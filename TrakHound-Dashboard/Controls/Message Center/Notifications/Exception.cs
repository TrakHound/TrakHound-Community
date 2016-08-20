// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace TrakHound_Dashboard.Notifications
{

    public class TH_Exception : Exception, ISerializable
    {

      public TH_Exception() { }

      public TH_Exception( string message ) : base( message ) { }

      public TH_Exception( string message, Exception inner ) : base( message, inner ) { }

      protected TH_Exception(SerializationInfo info,StreamingContext context ) : base( info, context ) { }

    }

}
