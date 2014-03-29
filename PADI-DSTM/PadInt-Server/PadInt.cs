﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadInt_Server {
    class PadInt {

        /* PadInt's uid */
        private int uid;

        /* PadInt's value during the transaction */
        private int actualValue;

        /* PadInt's value in the begining of the transaction */
        private int originalValue;

        /* Timer used in deadlock detection */
        //timer...

        /* uid of the next transaction to be promoted.
         *
         * Value -1 means that there is no transaction,
         *  identified by tid, waiting for promotion. 
         */
        private int promotion;

        /* Queue with transactions' tid with atributed read locks */
        private List<int> readers;

        /* Transaction' tid with atributed write lock.
         * 
         * Value -1 means that there is no transaction,
         *  identified by tid, writing. 
         */
        private int writer;

        /* Queue with transactions' tid with pending read locks */
        private List<int> pendingReaders;

        /* Queue with transactions' tid with pending write locks */
        private List<int> pendingWriters;

        /* uid represetns the PadInt's uid */
        protected PadInt (int uid)
        {

            this.uid = uid;
            this.actualValue = 0;
            this.originalValue = 0;
            //this.timer = ...
            this.promotion = -1;//confirmar???
            this.readers = new List<int>();
            this.writer = -1; //confirmar???
            this.pendingReaders = new List<int>();
            this.pendingWriters = new List<int>();
        }

        public int Uid
        {
            get { return uid; }
            set { this.uid = value; }
        }

        public int ActualValue
        {
            get { return actualValue; }
            set { this.actualValue = value; }
        }

        public int OriginalValue
        {
            get { return originalValue; }
            set { this.originalValue = value; }
        }

        /* public int Timer
        {
            get { return timer; }
            set { this.timer = value; }
        }*/

        /* Assigns to the transaction identified by tid
         * a read lock over the PadInt identified by uid,
         * as soon as possible.
         * 
         * Returns true if successful */
        public bool getReadLock (int tid, int uid)
        {

            /* ve se não há algum escritor 
             *  se nao existir mete nos leitores
               se existir escritor mete na fila de espera dos leitores
             */

            /* if there is no writer */
            if (writer > -1)
            {
                readers.Add(tid);
            }
            else
            {
                pendingReaders.Add(tid);
            }

            /* TODO
             * 
             * VER COMO E HISTORIA DE FICAR PARADO `A ESPERA SE SO´ 
             * RESPONDER DEPOIS DE TER O LOCK. MESMO QUE ISSO SEJA NO
             * SERVER TB TEM QUE SER VISTO AQUI DE ALGUMA FORMA*/

            return true;
        }

        /* Assigns to the transaction identified by tid
         * a write lock over the PadInt identified by uid,
         * as soon as possible.
         * 
         * Returns true if successful */
        public bool getWriteLock (int tid, int uid)
        {
            /* if don't exists a writer or readers */
            if (!(writer > -1 || readers.Count > 0))
            {
                writer = tid;
            }
            else
            {
                /* if the lock is a write lock */
                if (readers.Count == 0)
                {
                    /* if the lock is not assigned to the transaction
                     *  identified by tid */
                    if (writer != tid)
                    {
                        pendingWriters.Add(tid);
                    }
                }
                else
                {
                    /* if the locks are read locks */

                    /* if the transaction, identified by tid,
                     *  does not have a read lock */
                    if (!readers.Contains(tid))
                    {
                        pendingWriters.Add(tid);
                    }
                    else
                    {
                        /* if there is only a
                         *  reader (transaction identified by tid) */
                        if (readers.Count == 1)
                        {
                            writer = tid;
                        }
                        else
                        {
                            /* if there is no transaction wainting
                             * for promotion */
                            if (promotion == -1)
                            {
                                promotion = tid;
                            }
                            else
                            {
                                /* abort */
                                //TODO confirmar se basta retornar falso
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /* Frees a read lock over the PadInt, identified by uid,
        * owned by a transaction identified by tid.
        *
        * Returns true if successful */
        public bool freeReadLock (int tid, int uid)
        {
            readers.Remove(tid);
            dequeueReadLock(uid);
            return true;
        }

        /* Frees a write lock over the PadInt, identified by uid,
        * owned by a transaction identified with tid.
        *
        * Returns true if successful */
        public bool freeWriteLock (int tid, int uid)
        {
            /* "frees" writer variable */
            writer = -1;
            dequeueWriteLock(uid);
            return true;
        }

        /* 
        *  */
        private void dequeueReadLock (int uid)
        {
            /* quando tira do promotion meter a -1 */
        }

        /* 
        *  */
        private void dequeueWriteLock (int uid)
        {
            /* quando tira do promotion meter a -1 */
        }
    }
}