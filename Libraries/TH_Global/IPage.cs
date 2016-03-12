using System;
using System.ComponentModel;
using System.Windows.Media;

namespace TH_Global
{
    /// <summary>
    /// Interface used for any general page use that needs a name and image
    /// </summary>
    public interface IPage
    {

        string PageName { get; }

        ImageSource Image { get; }

        void Opened();
        bool Opening();

        void Closed();
        bool Closing();


        //event CancelEventHandler PageOpening;
        //event EventHandler PageOpened;
        
        //event CancelEventHandler PageClosing;
        //event EventHandler PageClosed;

    }

    //public delegate void EventHandler(IPage sender);
    //public delegate void CancelEventHandler
}
