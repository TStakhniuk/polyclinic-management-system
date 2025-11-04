using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DistrictPolyclinic.Pages;

namespace DistrictPolyclinic.Chat
{
    public static class SessionManager
    {
        // Save the message for the current session
        private static ObservableCollection<Message> _sessionMessages = new ObservableCollection<Message>();

        public static ObservableCollection<Message> SessionMessages
        {
            get { return _sessionMessages; }
            set { _sessionMessages = value; }
        }

        // Clear the message when the session ends
        public static void ClearSession()
        {
            _sessionMessages.Clear();
        }
    }
}
