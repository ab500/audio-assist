using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace Fusao.AudioAssist
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {

        private SoundPlayer.SoundPlayer _soundPlayer;

        /// <summary>
        /// Our main handle on Visual Studio, a reference to the top automation object.
        /// </summary>
        private DTE2 _applicationObject;
        private AddIn _addInInstance;

        /// <remarks>
        /// We keep these private references to these 
        /// objects around because there's some weirdness in the way
        /// they behave. Because they are COM objects they can be
        /// garbage collected, even if we've attached event handlers
        /// to them, and this will cause the events not to properly
        /// fire. 
        /// </remarks>
        #region CommandEvents handles
        private CommandEvents _breakpointsEvents;
        private CommandEvents _gotoDefinitionEvents;
        private CommandEvents _startEvents;
        private CommandEvents __findAllReferencesEvents;
        private CommandEvents _stopEvents;
        private DebuggerEvents _debuggerEvents;
        #endregion

        public Connect()
        {
            try
            {
                // Create a new sound player upon connection by the 
                // IDE. Throw a dialog failure message if unsuccessful.
                _soundPlayer = new SoundPlayer.SoundPlayer();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }


        public void OnConnection(object application,
            ext_ConnectMode connectMode,
            object addInInst,
            ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
        }

        public void OnDisconnection(ext_DisconnectMode disconnectMode,
            ref Array custom)
        {
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
            // Upon VS successfully starting up we add all the
            // event subscriptions neccessary to make AudioAssist work.
            AddSubscription();
        }

        public void OnBeginShutdown(ref Array custom)
        {
        }

        public void QueryStatus(string commandName,
            vsCommandStatusTextWanted neededText,
            ref vsCommandStatus status,
            ref object commandText)
        {

        }

        public void Exec(string commandName,
            vsCommandExecOption executeOption,
            ref object varIn,
            ref object varOut,
            ref bool handled)
        {

        }

        public void AddSubscription()
        {
            // "{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}, 1113" Guid-ID pair refer to
            // Project.AddReference command.
            // About how to get the Guid and ID of the specific command, please take a
            // look at this link on Dr.eX's blog:
            // http://blogs.msdn.com/dr._ex/archive/2007/04/17/using-enablevsiplogging-
            // to-identify-menus-and-commands-with-vs-2005-sp1.aspx
            try
            {

               _breakpointsEvents
                    = _applicationObject.Events.get_CommandEvents(
                        "{5EFC7975-14BC-11CF-9B2B-00AA00573819}",
                        769);
                _breakpointsEvents.BeforeExecute
                    += new _dispCommandEvents_BeforeExecuteEventHandler(breakpointsEvents_BeforeExecute);

                __findAllReferencesEvents
                    = _applicationObject.Events.get_CommandEvents(
                        "{5EFC7975-14BC-11CF-9B2B-00AA00573819}",
                        1915);
                __findAllReferencesEvents.BeforeExecute
                    += new _dispCommandEvents_BeforeExecuteEventHandler(findAllReferencesEvents_BeforeExecute);

                _gotoDefinitionEvents
                    = _applicationObject.Events.get_CommandEvents(
                        "{5EFC7975-14BC-11CF-9B2B-00AA00573819}",
                        935);
                _gotoDefinitionEvents.BeforeExecute
                    += new _dispCommandEvents_BeforeExecuteEventHandler(gotoDefinitionEvents_BeforeExecute);

                _stopEvents
                    = _applicationObject.Events.get_CommandEvents(
                        "{5EFC7975-14BC-11CF-9B2B-00AA00573819}",
                        179);
                _stopEvents.BeforeExecute
                    += new _dispCommandEvents_BeforeExecuteEventHandler(stopEvents_BeforeExecute);
                
                _startEvents
                    = _applicationObject.Events.get_CommandEvents(
                        "{5EFC7975-14BC-11CF-9B2B-00AA00573819}",
                        295);
                _startEvents.BeforeExecute
                    += new _dispCommandEvents_BeforeExecuteEventHandler(startEvents_BeforeExecute);

                _debuggerEvents = _applicationObject.Events.DebuggerEvents;  
                _debuggerEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(debuggerEvents_OnEnterBreakMode);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        #region Event Handlers
        private void startEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _soundPlayer.PlayNoise(SoundPlayer.SoundType.Start);
        }

        private void stopEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _soundPlayer.PlayNoise(SoundPlayer.SoundType.Stop);
        }

        private void gotoDefinitionEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _soundPlayer.PlayNoise(SoundPlayer.SoundType.GoToDefinition);
        }

        private void findAllReferencesEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _soundPlayer.PlayNoise(SoundPlayer.SoundType.FindAllReferences);
        }

        private void breakpointsEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            _soundPlayer.PlayNoise(SoundPlayer.SoundType.Breakpoint);
        }


        private void debuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            // In practice the enum Reason field doesn't seem to match up very well with 
            // reality, we've captured the observed enum values when these events actually occured
            // and used them instead of using the pre-set enum values. They are incorrect in many
            // cases.
            int intReason = (int) Reason;
            switch (intReason)
            {
                case 9: // breakpoint hit
                    _soundPlayer.PlayNoise(SoundPlayer.SoundType.BreakpointHit);
                    break;
                case 15: // exception hit
                    _soundPlayer.PlayNoise(SoundPlayer.SoundType.ExceptionHit);
                    break;
                case 8: // step
                    _soundPlayer.PlayNoise(SoundPlayer.SoundType.Step);
                    break;
            }
        }

        #endregion


    }
}