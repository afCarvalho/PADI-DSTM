﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonTypes;

namespace PadIntServer {
    class FrozenState : ServerState {

        /// <summary>
        /// Server's old state
        /// </summary>
        private ServerState oldState;
        private bool recover;

        internal FrozenState(Server server)
            : base(server) {
            oldState = server.State;
        }

        /// <summary>
        /// Freezed servers do nothing when this method is called
        /// </summary>
        internal override void ImAlive() {
            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                oldState.ImAlive();
                Monitor.Pulse(this);
            }
        }

        internal override bool CreatePadInt(int uid) {
            Logger.Log(new String[] { "FreezedServer", Server.ID.ToString(), "createPadInt", "uid ", uid.ToString() });
            bool result;

            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                result = oldState.CreatePadInt(uid);
                Monitor.Pulse(this);
            }
            return result;
        }

        internal override bool ConfirmPadInt(int uid) {
            Logger.Log(new String[] { "FreezedServer", Server.ID.ToString(), "confirmPadInt ", "uid", uid.ToString() });
            bool result;

            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                result = oldState.ConfirmPadInt(uid);
                Monitor.Pulse(this);
            }
            return result;
        }

        /* Returns the value of the PadInt when the transaction
         *  has the read/write lock.
         * Throw an exception if PadInt not found. 
         */
        internal override int ReadPadInt(int tid, int uid) {
            Logger.Log(new String[] { "FreezedServer", Server.ID.ToString(), "readPadInt ", "tid", tid.ToString(), "uid", uid.ToString() });
            int result;

            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                result = oldState.ReadPadInt(tid, uid);
                Monitor.Pulse(this);
            }
            return result;
        }

        internal override bool WritePadInt(int tid, int uid, int value) {
            Logger.Log(new String[] { "FreezedServer", Server.ID.ToString(), " writePadInt ", "tid", tid.ToString(), "uid", uid.ToString(), "value", value.ToString() });
            bool result;

            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                result = oldState.WritePadInt(tid, uid, value);
                Monitor.Pulse(this);
            }
            return result;
        }

        /// <summary>
        /// Commits a transaction on this server
        /// </summary>
        /// <param name="tid">transaction identifier</param>
        /// <param name="usedPadInts">Identifiers of PadInts involved</param>
        /// <returns>A predicate confirming the sucess of the operations</returns>
        internal override bool Commit(int tid, List<int> usedPadInts) {
            Logger.Log(new String[] { "FreezedServer", Server.ID.ToString(), "commit", "tid", tid.ToString() });
            bool result;

            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                result = oldState.Commit(tid, usedPadInts);
                Monitor.Pulse(this);
            }
            return result;
        }

        /// <summary>
        /// Aborts a transaction on this server
        /// </summary>
        /// <param name="tid">transaction identifier</param>
        /// <param name="usedPadInts">Identifiers of PadInts involved</param>
        /// <returns>A predicate confirming the sucess of the operations</returns>
        internal override bool Abort(int tid, List<int> usedPadInts) {
            Logger.Log(new String[] { "FreezedServer", Server.ID.ToString(), "abort", "tid", tid.ToString() });
            bool result;

            lock(this) {
                while(!recover) {
                    Monitor.Wait(this);
                }
                result = oldState.Abort(tid, usedPadInts);
                Monitor.Pulse(this);
            }
            return result;
        }

        internal override bool Recover() {
            lock(this) {
                recover = true;
                Monitor.Pulse(this);
            }
            return true;
        }
    }
}
