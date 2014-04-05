﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;


namespace PadIntServer {

    class Server : MarshalByRefObject, IServer {

        /* Pending request list */
        //private List<Request> requestList = new List<Request>();

        /* Structure that maps UID to PadInt */
        private Dictionary<int, IPadInt> padIntDict;
        private int id;
        private int maxCapacity;
        ILog log;
        IMaster masterServer;

        public Server() {
            padIntDict = new Dictionary<int, IPadInt>();
        }

        /* Obtain the PadInt identified by uid.
         * Returns null if not found. 
         */
        private IPadInt getPadInt(int uid) {
            log.log(new String[] { "server", "getPadInt", "uid", uid.ToString() });

            foreach(KeyValuePair<int, IPadInt> entry in padIntDict) {
                if(entry.Key == uid) {
                    return entry.Value;
                }
            }

            return null;
        }

        public bool createPadInt(int uid) {
            log.log(new String[] { "server", id.ToString(), "createPadInt", "uid ", uid.ToString() });

            try {


                if(padIntDict.Count<2*maxCapacity) {
                    padIntDict.Add(uid, new PadInt(uid));
                } else {

                    Dictionary<int, String> serverAddresses = masterServer.getServersInfo(true).Item1;
                    movePadInts(serverAddresses);
                }

                return true;
            } catch(ArgumentException) {
                return false;
            }


        }

        public void movePadInts(Dictionary<int, String> serverAddresses) {
            log.log(new String[] { "server", id.ToString(), "movePadInts" });

            Dictionary<int, IPadInt> sparePadInts = new Dictionary<int, IPadInt>();
            int originalCapacity = maxCapacity;


            if(id == 0)
                return;
            while(sparePadInts.Count < originalCapacity/2) {
                maxCapacity=2*maxCapacity;
                for(int i=0; i < padIntDict.Count; i++) {
                    if(padIntDict.ContainsKey(i) && i<maxCapacity*id) {
                        sparePadInts.Add(i, padIntDict[i]);
                        padIntDict.Remove(i);
                    }
                }
            }





            string leftServerAddress = serverAddresses[id-1];
            log.log(new String[] { "left server Address", "new capacity", maxCapacity.ToString() });

            IServer server = (IServer)Activator.GetObject(typeof(IServer), leftServerAddress);
            server.attachPadInts(serverAddresses, sparePadInts);





        }

        public void attachPadInts(Dictionary<int, String> serverAddresses, Dictionary<int, IPadInt> sparedPadInts) {
            log.log(new String[] { "Server", id.ToString(), "attachPadInts" });



            for(int i =0; i <sparedPadInts.Count; i++) {
                if(sparedPadInts.ContainsKey(i))
                    padIntDict.Add(i, sparedPadInts[i]);
            }

            if(id == 0)
                return;

            movePadInts(serverAddresses);

        }

        public void setMaxCapacity(int value) {
            maxCapacity = value;
        }

        public bool confirmPadInt(int uid) {
            log.log(new String[] { "Server", id.ToString(), "confirmPadInt ", "uid", uid.ToString() });
            return padIntDict.ContainsKey(uid);
        }

        /* Returns the value of the PadInt when the transaction
         *  has the read/write lock.
         * Throw an exceptio if PadInt not found. 
         */
        public int readPadInt(int tid, int uid) {
            throw new NotImplementedException();


            /* Obtain the PadInt identified by uid */
            PadInt padInt = (PadInt)getPadInt(uid);

            if(padInt != null) {
                if(padInt.hasWriteLock(tid) || padInt.getReadLock(tid)) {
                    return padInt.ActualValue;
                }
            } else {
                //throw new PadIntNotFoundException() ;
            }
        }

        public bool writePadInt(int tid, int uid, int value) {
            log.log(new String[] { " Server ", id.ToString(), " writePadInt ", "tid", tid.ToString(), "uid", uid.ToString(), "value", value.ToString() });


            /* Obtain the PadInt identified by uid */
            PadInt padInt = (PadInt)getPadInt(uid);

            /* TODO
             * tem que se ver o caso em que se vai guardar
             * o valor original a ser usado no abort 
             */
            if(padInt != null) {
                if(padInt.getWriteLock(tid)) {
                    padInt.ActualValue = value;
                    return true;
                }
            } else {
                //throw new PadIntNotFoundException() ;
            }

            /* e´preciso? */
            return false;
        }

        /* usedPadInts sao os uid usados pela transacao tid */
        public void commit(int tid, List<int> usedPadInts) {
            throw new NotImplementedException();
            /* TODO !!!!!
             * 
             * se por acaso usarmos o tab no cliente para guardar valores para
             *  evitar andar a fazer varias chamadas remotas se calhar mete-se
             *  no cliente que ao chamar o commit (do cliente) esse metodo chama
             *  primeiro os writes para todos os PadInt que escreveu para assim
             *  actualizar no server.
             */

            /* ainda nao esta acabado */

            foreach(int padInt in usedPadInts) {

                /* PadInt padInt = getPadInt(uid);
                 * 
                 * chama metodo commit do PadInt que faz:
                 * 
                 * liberta locks de read: freeReadLock(int tid)
                 * liberta locks de write: freeWriteLock(int tid)
                 * 
                 * verifica se o tid da transaccao nao esta na promotion
                 *  - se estiver e for commit manda abort????
                 *  - se estiver e for abort limpa apenas
                 * 
                 * apenas precisa de fazer isto apenas quando sao escritas:
                 * entry.Value.OriginalValue = entry.Value.ActualValue;
                 
                 */

            }

            /* retorna algum tipo de msg a indicar que fez o commit? */
        }

        /* usedPadInts sao os uid usados pela transacao tid */
        public void abort(int tid, List<int> usedPadInts) {
            throw new NotImplementedException();
            /* TODO !!!!!
             * 
             * se por acaso usarmos o tab no cliente para guardar valores para
             *  evitar andar a fazer varias chamadas remotas se calhar mete-se
             *  no cliente que ao chamar o commit (do cliente) esse metodo chama
             *  primeiro os writes para todos os PadInt que escreveu para assim
             *  actualizar no server.
             */

            /* ainda nao esta acabado */
            foreach(int padInt in usedPadInts) {

                /* PadInt padInt = getPadInt(uid);
                 * 
                 * chama metodo abort do PadInt que faz:
                 * 
                 * liberta locks de read: freeReadLock(int tid)
                 * liberta locks de write: freeWriteLock(int tid)
                 * 
                 * verifica se o tid da transaccao nao esta na promotion
                 *  - se estiver e for commit manda abort????
                 *  - se estiver e for abort limpa apenas
                 * 
                 * apenas e so´ no caso em que era lock de write e´ que faz isto:
                 * entry.Value.ActualValue = entry.Value.OriginalValue;
                 
                 */
            }

            /* retorna algum tipo de msg a indicar que fez o abort? */
        }

        internal void setID(int serverID) {
            this.id=serverID;
        }

        internal void setMaster(IMaster master) {
            masterServer = master;
        }

        internal void setLog(ILog log) {
            this.log= log;
        }

    }
}
