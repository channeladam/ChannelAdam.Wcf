////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text;
////using System.Threading;
////using System.Threading.Tasks;
////using System.ServiceModel;

////namespace ChannelAdam.Wcf.BehaviourSpecs
////{
////    public class CorrectlyUseWcfClientDemo
////    {
////        public void CallServiceOperation()
////        {
////            SampleServiceClient client = null;

////            try
////            {
////                client = new SampleServiceClient();

////                var response = client.SampleOperation(1234);

////                // Do some business logic
////            }
////            catch (FaultException<MyCustomException>)
////            {
////                // Do some business logic for this SOAP Fault Exception
////            }
////            catch (FaultException)
////            {
////                // Do some business logic for this SOAP Fault Exception
////            }
////            catch (CommunicationException)
////            {
////                // Catch this expected exception so it is not propagated further.
////                // Perhaps write this exception out to log file for gathering statistics...
////            }
////            catch (TimeoutException)
////            {
////                // Catch this expected exception so it is not propagated further.
////                // Perhaps write this exception out to log file for gathering statistics...
////            }
////            catch (Exception)
////            {
////                // An unexpected exception that we don't know how to handle.
////                // Perhaps write this exception out to log file for support purposes...
////                throw;
////            }
////            finally
////            {
////                // This will:
////                // - be executed if any exception was thrown above in the 'try' (including ThreadAbortException); and
////                // - ensure that CorrectlyCloseServiceChannel() itself will not be interrupted by a ThreadAbortException
////                //   (since it is executing from within a 'finally' block)
////                CorrectlyCloseOrAbortServiceChannel(client);

////                // Unreference the client
////                client = null;
////            }
////        }
        
////        private void CorrectlyCloseOrAbortServiceChannel(ICommunicationObject communicationObject)
////        {
////            bool isClosed = false;

////            if (communicationObject == null || communicationObject.State == CommunicationState.Closed)
////            {
////                return;
////            }

////            try 
////            {
////                if (communicationObject.State != CommunicationState.Faulted)
////                {
////                    communicationObject.Close();
////                    isClosed = true;
////                }
////            }
////            catch (CommunicationException)
////            {
////                // Catch this expected exception so it is not propagated further.
////                // Perhaps write this exception out to log file for gathering statistics...
////            }
////            catch (TimeoutException)
////            {
////                // Catch this expected exception so it is not propagated further.
////                // Perhaps write this exception out to log file for gathering statistics...
////            }
////            catch (Exception)
////            {
////                // An unexpected exception that we don't know how to handle.
////                // Perhaps write this exception out to log file for support purposes...
////                throw;
////            }
////            finally
////            {
////                // If State was Faulted or any exception occurred while doing the Close(), then do an Abort()
////                if (!isClosed)
////                {
////                    AbortServiceChannel(communicationObject);
////                }
////            }
////        }

////        private static void AbortServiceChannel(ICommunicationObject communicationObject)
////        {
////            try
////            {
////                communicationObject.Abort();
////            }
////            catch (Exception)
////            {
////                // An unexpected exception that we don't know how to handle.
////                // If we are in this situation (or any exception above):
////                // - things are bad - very bad - even an Abort() doesn't work!
////                // - we should NOT retry the Abort() because it has already failed and there is nothing to suggest it could be successful next time
////                //
////                // Perhaps write this exception out to log file for support purposes...
////                throw;
////            }
////        }

////    }
////}
