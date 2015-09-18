// CRI.CustomResourceInterceptor
//
//  Author:     Garison E Piatt
//  Contact:    {removed}
//  Created:    11/17/14
//  Version:    1.0.0
//
// Apparently, when Awesomium was first created, the programmers did not understand that someone would
// eventually want to post data from the application.  So they made it incredibly difficult to upload
// POST parameters to the remote web site.  We have to jump through hoops to get that done.
//
// This module provides that hoop-jumping in a simple-to-understand fashion.  We hope.  It overrides
// the current resource interceptor (if any), replacing both the OnRequest and OnFilterNavigation
// methods (we aren't using the latter yet).
//
// It also provides settable parameters.  Once this module is attached to the WebCore, it is *always*
// attached; therefore, we can simply change the parameters before posting to the web site.
//
// File uploads are currently unhandled, and, once handled, will probably only upload one file.  We
// will deal with that issue later.
//
// To incoroprate this into your application, follow these steps:
//  1.  Add this file to your project.  You know how to do that.
//  2.  Edit your MainWindow.cs file.
//      a.  At the top, add:
//              using CRI;
//      b.  inside the main class declaration, near the top, add:
//              private CustomResourceInterceptor cri;
//      c.  In the MainWindow method, add:
//              WebCore.Started += OnWebCoreOnStarted;
//              cri = new CustomResourceInterceptor();
//          and (set *before* you set the Source value for the Web Control):
//              cri.Enabled = true;
//              cri.Parameters = String.Format("login={0}&password={1}", login, pw);
//          (Choose your own parameters, but format them like a GET query.)
//      d.  Add the following method:
//              private void OnWebCoreOnStarted(object sender, CoreStartEventArgs coreStartEventArgs) {
//                  WebCore.ResourceInterceptor = cri;
//              }
//  3. Compile your application.  It should work.

using System;
using System.Runtime.InteropServices;
using System.Text;
using Awesomium.Core;
using Awesomium.Windows.Controls;


namespace CRI
{
    //*  CustomResourceInterceptor
    //  This object replaces the standard Resource Interceptor (if any; we still don't know) with something
    //  that allows posting data to the remote web site.  It overrides both the OnRequest and OnFilterNavigation
    //  methods.  Public variables allow for run-time configuration.
    public class CustomResourceInterceptor : IResourceInterceptor
    {
        // Since the default interceptor remains overridden for the remainder of the session, we need to disable
        // the methods herein unless we are actually using them.  Note that both methods are disabled by default.
        public bool RequestEnabled = false;
        public bool FilterEnabled = false;

        // These are the parameters we send to the remote site.  They are empty by default; another safeguard
        // against sending POST data unnecessarily.  Currently, both values allow for only one string.  POST
        // variables can be combined (by the caller) into one string, but this limits us to only one file
        // upload at a time.  Someday, we will have to fix that.  And make it backward-compatible.
        public String Parameters = null;
        public String FilePath = null;

        /** OnRequest
        ** This ovverrides the default OnRequest method of the standard resource interceptor.  It receives
        ** the resource request object as a parameter.
        **
        ** It first checks whether or not it is enabled, and returns NULL if not.  Next it sees if any 
        ** parameters are defined.  If so, it converst them to a byte stream and appends them to the request.
        ** Currently, files are not handled, but we hope to add that someday.
        */
        public ResourceResponse OnRequest(ResourceRequest request)
        {
            // We do nothing at all if we aren't enabled.  This is a stopgap that prevents us from sending
            // POST data with every request.
            if (RequestEnabled == false) return null;

            // If the Parameters are defined, convert them to a byte stream and append them to the request.
            if (Parameters != null)
            {
                var str = Encoding.Default.GetBytes(Parameters);
                var bytes = Encoding.UTF8.GetString(str);

                request.AppendUploadBytes(bytes, (uint)bytes.Length);
            }

            // If either the parameters or file path are defined, this is a POST request.  Someday, we'll
            // figure out how to get Awesomium to understand Multipart Form data.
            if (Parameters != null || FilePath != null)
            {
                request.Method = "POST";
                request.AppendExtraHeader("Content-Type", "application/x-www-form-urlencoded"); //"multipart/form-data");
            }

            // Once the data has been appended to the page request, we need to disable this process.  Otherwise,
            // it will keep adding the data to every request, including those that come from the web site.
            RequestEnabled = false;
            Parameters = null;
            FilePath = null;

            return null;
        }


        /** OnFilterNavigation
        ** Not currently used, but needed to keep VisualStudio happy.
        */
        public bool OnFilterNavigation(NavigationRequest request)
        {
            return false;
        }
    }
}
