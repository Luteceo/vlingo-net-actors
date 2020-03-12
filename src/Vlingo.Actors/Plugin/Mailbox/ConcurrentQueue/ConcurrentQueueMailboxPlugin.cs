﻿// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailboxPlugin : AbstractPlugin, IMailboxProvider
    {
        private readonly ConcurrentQueueMailboxPluginConfiguration configuration;
        private IDispatcher? executorDispatcher;

        public ConcurrentQueueMailboxPlugin()
        {
            configuration = ConcurrentQueueMailboxPluginConfiguration.Define();
        }

        private ConcurrentQueueMailboxPlugin(IPluginConfiguration configuration)
        {
            this.configuration = (ConcurrentQueueMailboxPluginConfiguration)configuration;
        }

        public override string Name => configuration.Name;

        public override int Pass => 1;

        public override IPluginConfiguration Configuration => configuration;

        public override void Start(IRegistrar registrar)
        {
            executorDispatcher = new ExecutorDispatcher(System.Environment.ProcessorCount, configuration.NumberOfDispatchersFactor);
            registrar.Register(configuration.Name, configuration.IsDefaultMailbox, this);
        }

        public IMailbox ProvideMailboxFor(int hashCode) 
            => new ConcurrentQueueMailbox(executorDispatcher!, configuration.DispatcherThrottlingCount);

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            return new ConcurrentQueueMailbox(dispatcher, configuration.DispatcherThrottlingCount);
        }

        public override void Close() => executorDispatcher!.Close();

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new ConcurrentQueueMailboxPlugin(overrideConfiguration);
    }
}
