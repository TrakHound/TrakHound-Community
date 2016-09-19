//Copyright(c) 2015, lduchosal
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//* Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following disclaimer.


//* Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation

// and/or other materials provided with the distribution.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
//CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
//OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections;
using System.Numerics;

namespace System.Net {
    public class IPAddressCollection : IEnumerable<IPAddress>, IEnumerator<IPAddress> {

        private IPNetwork _ipnetwork;
        private BigInteger _enumerator;

        internal IPAddressCollection(IPNetwork ipnetwork) {
            this._ipnetwork = ipnetwork;
            this._enumerator = -1;
        }


        #region Count, Array, Enumerator

        public BigInteger Count {
            get {
                return this._ipnetwork.Total;
            }
        }

        public IPAddress this[BigInteger i] {
            get {
                if (i >= this.Count) {
                    throw new ArgumentOutOfRangeException("i");
                }
                byte width = this._ipnetwork.AddressFamily == Sockets.AddressFamily.InterNetwork ? (byte)32 : (byte)128;
                IPNetworkCollection ipn = IPNetwork.Subnet(this._ipnetwork, width);
                return ipn[i].Network;
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator<IPAddress> IEnumerable<IPAddress>.GetEnumerator() {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this;
        }

        #region IEnumerator<IPNetwork> Members

        public IPAddress Current {
            get { return this[this._enumerator]; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            // nothing to dispose
            return;
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get { return this.Current; }
        }

        public bool MoveNext() {
            this._enumerator++;
            if (this._enumerator >= this.Count) {
                return false;
            }
            return true;

        }

        public void Reset() {
            this._enumerator = -1;
        }

        #endregion

        #endregion
    }
}
