﻿// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors
{
    public sealed class DeadLettersActor : Actor, IDeadLetters
    {
        private readonly IList<IDeadLettersListener> listeners;

        public DeadLettersActor()
        {
            listeners = new List<IDeadLettersListener>();
            Stage.World.DeadLetters = SelfAs<IDeadLetters>();
        }

        public void FailedDelivery(DeadLetter deadLetter)
        {
            Logger.Debug(deadLetter.ToString());

            foreach (var listener in listeners)
            {
                try
                {
                    listener.Handle(deadLetter);
                }
                catch (Exception ex)
                {
                    // ignore, but log
                    Logger.Warn($"DeadLetters listener failed to handle: {deadLetter}", ex);
                }
            }
        }

        public void RegisterListener(IDeadLettersListener listener)
        {
            listeners.Add(listener);
        }

        protected internal override void AfterStop()
        {
            Stage.World.DeadLetters = null;
            base.AfterStop();
        }
    }
}