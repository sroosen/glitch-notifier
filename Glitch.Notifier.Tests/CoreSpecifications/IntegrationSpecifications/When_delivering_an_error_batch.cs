using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glitch.Notifier.Notifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glitch.Notifier.Tests.CoreSpecifications.IntegrationSpecifications
{
    [TestClass]
    public class When_delivering_an_error_batch
    {
        private readonly TaskCompletionSource<ErrorBatchDeliveryInfo> _errorDeliveryTaskCompletionSource = new TaskCompletionSource<ErrorBatchDeliveryInfo>();

        private Task<ErrorBatchDeliveryInfo> ErrorDeliveryTask
        {
            get { return _errorDeliveryTaskCompletionSource.Task; }
        }

        [TestMethod]
        public void Should_succeed()
        {
            Glitch.Config.WithApiUrl("http://localhost:1289/v1/errors")
                .WithApiKey("6b36472946f04a32805a35fead592856")
                .WithNotificationsMaxBatchSize(2);
            ErrorQueue.Clear();

            Glitch.Notifications.OnBatchDelivered += Instance_OnBatchDelivered;
            try
            {
                throw new InvalidOperationException("test1");
            }
            catch (Exception ex)
            {
                Glitch.Factory.Error(ex)
                            .With("TestKey", "TestValue")
                            .WithUser("testUser1")
                            .WithLocation("http://test/12234")
                            .Send();
            }

            Glitch.Factory.Error(new ArgumentException("test2"))
                          .WithUser("testUser2")
                          .WithLocation("Controller1#Action2")
                          .Send();

            Assert.IsTrue(ErrorDeliveryTask.Wait(TimeSpan.FromMinutes(1)));
            Glitch.Notifications.Stop(TimeSpan.Zero);
            Glitch.Notifications.OnBatchDelivered -= Instance_OnBatchDelivered;
            var info = ErrorDeliveryTask.Result;
            Assert.AreEqual(2, info.ErrorBatch.Errors.Length);
            Assert.IsNull(info.Exception);
            Assert.IsTrue(info.Succeeded);
        }

        void Instance_OnBatchDelivered(ErrorBatchDeliveryInfo info)
        {
            _errorDeliveryTaskCompletionSource.SetResult(info);
        }
    }
}
