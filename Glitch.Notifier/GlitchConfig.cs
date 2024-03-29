﻿using System;
using System.Threading;
using Glitch.Notifier.ErrorContentFilters;
using Glitch.Notifier.ErrorFilters;

namespace Glitch.Notifier
{
    public class GlitchConfig
    {
        public GlitchConfig(GlitchConfigSection section)
        {
            if (section == null) return;
            WithApiUrl(section.ApiUrl);
            WithApiKey(section.ApiKey);
            WithNotificationsMaxBatchSize(section.NotificationsMaxBatchSize);
            WithNotificationsMaxInterval(TimeSpan.FromMinutes(section.NotificationsMaxIntervalInMinutes));
            SendNotifications(section.Notify);
            foreach(var contentFilter in section.IgnoreContent)
            {
                IgnoreContent.FromDataGroupWithKeysContaining(contentFilter.DataGroup, contentFilter.KeyContains);
            }
            foreach(var exceptionFilter in section.IgnoreErrors)
            {
                IgnoreErrors.WithFilter(exceptionFilter.CreateFilter());
            }
        }

        public GlitchConfig WithApiKey(string apiKey)
        {
            ApiKey = apiKey;
            return this;
        }

        public GlitchConfig WithApiUrl(string apiUrl)
        {
            ApiUrl = apiUrl;
            return this;
        }

        public GlitchConfig SendNotifications(bool notify)
        {
            Notify = notify;
            return this;
        }

        public GlitchConfig WithDefaultErrorProfile(string errorProfile)
        {
            ErrorProfile = errorProfile;
            return this;
        }

        private Func<Error, string> _groupKeyGenerator = GroupKeyDefaultGenerator.Compute;
        public Func<Error, string> GroupKeyGenerator
        {
            get { return _groupKeyGenerator; }
        }

        public GlitchConfig WithGroupKeyGenerator(Func<Error, string> groupKeyFunc)
        {
            if (groupKeyFunc == null) throw new ArgumentNullException("groupKeyFunc");
            _groupKeyGenerator = groupKeyFunc;
            return this;
        }

        public Func<string> CurrentUserRetriever { get; private set; }
        public GlitchConfig WithCurrentUser(Func<string> currentUserFunc)
        {
            if (currentUserFunc == null) throw new ArgumentNullException("currentUserFunc");
            CurrentUserRetriever = currentUserFunc;
            return this;
        }

        public string ApiUrl { get; private set; }

        public string ApiKey { get; private set; }

        public bool IsHttps { get; private set; }

        private bool _notify = true;
        public bool Notify
        {
            get { return _notify; }
            private set { _notify = value; }
        }

        private string _errorProfile = "glitch-v1-net-default";
        public string ErrorProfile
        {
            get { return _errorProfile; }
            private set { _errorProfile = value; }
        }

        private readonly ErrorFilter _ignoreErrors = new ErrorFilter();
        public ErrorFilter IgnoreErrors
        {
            get { return _ignoreErrors; }
        }

        private readonly ErrorContentFilter _errorContentFilter = new ErrorContentFilter();

        public ErrorContentFilter IgnoreContent
        {
            get { return _errorContentFilter; }
        }

        private int _notificationMaxBatchSize = 10;
        public int NotificationsMaxBatchSize
        {
            get { return _notificationMaxBatchSize; }
        }

        public GlitchConfig WithNotificationsMaxBatchSize(int maxBatchSize)
        {
            Interlocked.Exchange(ref _notificationMaxBatchSize, maxBatchSize);
            return this;
        }

        private TimeSpan _notificationsMaxInterval = TimeSpan.FromMinutes(1);
        private readonly ReaderWriterLockSlim _maxIntervalLock = new ReaderWriterLockSlim();
        public TimeSpan NotificationsMaxInterval
        {
            get
            {
                _maxIntervalLock.EnterReadLock();
                try
                {
                    return _notificationsMaxInterval;
                }
                finally
                {
                    _maxIntervalLock.ExitReadLock();
                }
            }
        }

        public GlitchConfig WithNotificationsMaxInterval(TimeSpan maxInterval)
        {
            _maxIntervalLock.EnterWriteLock();
            try
            {
                _notificationsMaxInterval = maxInterval;
                return this;
            }
            finally
            {
                _maxIntervalLock.ExitWriteLock();
            }
        }
    }
}
