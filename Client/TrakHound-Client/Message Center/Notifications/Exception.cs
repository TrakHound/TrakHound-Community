// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace TrakHound_Client.Notifications
{

    public class TH_Exception : Exception, ISerializable
    {

      public TH_Exception() { }

      public TH_Exception( string message ) : base( message ) { }

      public TH_Exception( string message, System.Exception inner ) : base( message, inner ) { }

      protected TH_Exception(SerializationInfo info,StreamingContext context ) : base( info, context ) { }

    }

}
