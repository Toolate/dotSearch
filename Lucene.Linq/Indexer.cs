using System;
using System.Collections.Generic;
using System.Threading;
using Lucene.Net.Index;
using Lucene.Net.Store;


namespace Lucene.Linq
{
    
    /// <summary>
    /// Abstract base indexer for executing an indexer hierarchy.
    /// By extending these classes, developers may choose their own with parallel indexing strategy.
    /// These indexing "runners" are thread-safe.
    /// They also follow the Composite and Disposal patterns. 
    /// </summary>
    public abstract class Indexer<TIndex> : IDisposable {
        #region Fields
        
        private string name = "Default Indexer";
        private readonly object stopLock = new object();
        private bool stopping = false;
        private bool stopped = false;
        private readonly IIndex<TIndex> index;

        #endregion

        #region Ctors

        ///<summary>
        /// Creates an indexer around an index
        ///</summary>
        ///<param name="index">The index instance</param>
        protected Indexer(IIndex<TIndex> index) {
            if (index == null) {
                throw new ArgumentNullException("index");
            }
            this.index = index;
        }

        #endregion

        #region Properties

        ///<summary>
        /// The name of the indexer
        ///</summary>
        public String Name
        {
            get
            {
                return name;
            }
            protected set {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                name = value;
            }
        }

        ///<summary>
        /// The underlying index
        ///</summary>
        public IIndex<TIndex> Index {
            get { return index; }
        }

        ///<summary>
        /// Children indexers
        ///</summary>
        public virtual IEnumerable<Indexer<TIndex>> Children
        {
            get
            {
                return null;
            }
        }

        ///<summary>
        /// Gets the stopping flag. True if the indexer is in a stopping state.
        ///</summary>
        public virtual bool Stopping
        {
            get
            {
                lock (stopLock)
                {
                    return stopping;
                }
            }
        }

        ///<summary>
        /// Gets the stopped flag. True if the indexer is in a stopped state.
        ///</summary>
        public virtual bool Stopped
        {
            get
            {
                lock (stopLock)
                {
                    return stopped;
                }

            }
            protected set {
                
                lock (stopLock) {
                    if (value) {
                        stopped = value;
                    }
                }
            }

        }

        ///<summary>Returns true if all children have stopped</summary>
        public bool IsChildrenStopped {
            get {

                // if any child indexers are false, return false
                foreach (var c in Children) {
                    if (!(c.Stopped))
                        return false;
                }

                // otherwise. return true
                return true;
            }
        }

        ///<summary>Returns true if all children are stopping</summary>
        public bool IsChildrenStopping {
            get {

                // if any child indexers are not stopping, return false
                foreach (var c in Children) {
                    if (!(c.Stopping))
                        return false;
                }

                // otherwise. return true
                return true;
            }
        }

        #endregion

        #region Run/Stop

        /// <summary>Runs the indexer</summary>
        public virtual void Run()
        {
            throw new NotSupportedException();
        }

        /// <summary>Sets stopping flag for safe stopping</summary>
        public virtual void Stop()
        {
            lock (stopLock)
            {
                stopping = true;
            }
        }

        #endregion

        #region Add/Remove Children

        /// <summary>Adds a child indexer. Only in CompositeIndexer derived classes
        /// </summary>
        /// <param name="indexer">New child indexer</param>
        public virtual void AddChild(Indexer<TIndex> indexer)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes a child indexer. Only in CompositeIndexer derived classes
        /// </summary>
        /// <param name="indexer">Child indexer to remove</param>
        public virtual void RemoveChild(Indexer<TIndex> indexer)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Index Operations

        protected void Merge(Directory[] directories) {
            Index.Context.Merge(directories);
        }

        #endregion

        #region Disposal pattern

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">true when a class in the same hierarchy is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed objects
            }
            // free unmanaged objects
        }

        /// <summary>
        /// Disposes everything and suppresses finalizer
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalize destructor called by CLR
        /// </summary>
        ~Indexer()
        {
            Dispose(false);
        }

        #endregion
    }

    /// <summary>Abstract batch indexer that executes children sequentially</summary>
    public abstract class CompositeIndexer<TIndex> : Indexer<TIndex> {
        #region Children
        private readonly List<Indexer<TIndex>> children = new List<Indexer<TIndex>>();


        ///<summary>
        /// Children indexers
        ///</summary>
        public override IEnumerable<Indexer<TIndex>> Children
        {
            get
            {
                return this.children;
            }
        }
        
        /// <summary>Adds a child indexer</summary>
        /// <param name="indexer">child indexer</param>
        public override void AddChild(Indexer<TIndex> indexer) {
            //indexer.AppendIndex = this.appendIndex;
            children.Add(indexer);
        }

        /// <summary>Removes a child indexer</summary>
        /// <param name="indexer">child indexer to remove</param>
        public override void RemoveChild(Indexer<TIndex> indexer) {
            if (!children.Remove(indexer)) {
                Console.WriteLine(indexer.Name + " is not a valid child and therefore not removed");
            }
        }

        #endregion

        #region Ctor

        /// <summary>Constructs an indexer with no child indexers</summary>
        protected CompositeIndexer(IIndex<TIndex> index):base(index)
        {
            Name = "Composite Indexer";
        }

        #endregion

        #region Properties

        ///<summary>Gets the stopping flag. True if the indexer is in a stopping state.</summary>
        public override bool Stopping
        {
            get
            {
                return IsChildrenStopping;
            }
        }

        ///<summary>Gets the stopped flag. True if the indexer is in a stopped state.</summary>
        public override bool Stopped
        {
            get
            {
                return IsChildrenStopped;
            }
        }

        #endregion

        #region Run/Stop

        /// <summary>Runs each child indexer one after the other</summary>
        public override void Run()
        {
            foreach (Indexer<TIndex> i in children)
            {
                // Break the execution loop if Stopping was signaled
                if (Stopping)
                    break;

                try
                {
                    i.Run();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

        }

        /// <summary>Calls Stop() on each child indexer</summary>
        public override void Stop()
        {
            foreach (Indexer<TIndex> i in children)
            {
                i.Stop();
            }
        }

        #endregion

        #region Disposal

        /// <summary>Disposes of managed and unmanaged resources</summary>
        /// <param name="disposing">true when a class in the same hierarchy is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release managed resources
                foreach (Indexer<TIndex> i in children)
                {
                    i.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }


    ///<summary>
    /// Runs child indexers sequentially i.e. one after the other in order
    ///</summary>
    ///<typeparam name="TIndex">The type of the index</typeparam>
    public class SequentialIndexer<TIndex> : CompositeIndexer<TIndex> {
        #region Ctor

        ///<summary>
        /// Constructs a sequential indexer around an index
        ///</summary>
        ///<param name="index">The index instance</param>
        public SequentialIndexer(IIndex<TIndex> index):base(index)
        {
            Name = "Sequential Indexer";
        }

        #endregion
    }

    /// <summary>
    /// The Parallel Indexer will run all child indexers in parallel in separate threads
    /// </summary>
    public class ParallelIndexer<TIndex> : Indexer<TIndex>
    {

        private readonly List<Indexer<TIndex>> children = new List<Indexer<TIndex>>();

        ///<summary>
        /// Children indexers
        ///</summary>
        public override IEnumerable<Indexer<TIndex>> Children
        {
            get
            {
                return this.children;
            }
        }

        ///<summary>
        /// Gets the stopping flag. True if the indexer is in a stopping state.
        ///</summary>
        public override bool Stopping
        {
            get
            {

                // if any child indexers are false, return false
                for (int i = 0; i < children.Count; i++)
                {
                    if (!(children[i].Stopping))
                        return false;
                }

                // otherwise. return true
                return true;
            }
        }

        ///<summary>
        /// Gets the stopped flag. True if the indexer is in a stopped state.
        ///</summary>
        public override bool Stopped
        {
            get
            {

                // if any child indexers are false, return false
                for (int i = 0; i < children.Count; i++)
                {
                    if (!(children[i].Stopped))
                        return false;
                }

                // otherwise. return true
                return true;
            }
        }

        /// <summary>
        /// Constructs a new parallel indexer with no children
        /// </summary>
        public ParallelIndexer(IIndex<TIndex> index):base(index)
        {
            Name = "Parallel Indexer";
        }

        /// <summary>
        /// Starts running all child indexers in separate threads, and waits until they all are stopped.
        /// </summary>
        public override void Run()
        {

            // start all child indexers in seperate threads
            foreach (Indexer<TIndex> i in children)
            {
                try
                {
                    var t = new Thread(i.Run);
                    t.Start();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            Thread.Sleep(2000);

            // wait for children to stop
            while (!IsChildrenStopped)
            {
                Thread.Sleep(1000);
            }

            
        }

        /// <summary>
        /// Calls Stop() on all child indexers
        /// </summary>
        public override void Stop()
        {
            foreach (Indexer<TIndex> i in children)
            {
                i.Stop();
            }
        }

        /// <summary>
        /// Adds a child indexer
        /// 
        /// The new child indexer is given the same appendIndex value as this object
        /// </summary>
        /// <param name="indexer">new child indexer</param>
        public override void AddChild(Indexer<TIndex> indexer)
        {
            children.Add(indexer);
        }

        /// <summary>
        /// Removes a child indexer
        /// </summary>
        /// <param name="indexer">child indexer to remove</param>
        public override void RemoveChild(Indexer<TIndex> indexer)
        {
            if (!children.Remove(indexer))
            {
                Console.WriteLine(indexer.Name + " is not a valid child and therefore not removed");
            }
        }

        #region disposal

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing">true when a class in the same hierarchy is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release managed resources
                foreach (var i in children)
                {
                    i.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}
