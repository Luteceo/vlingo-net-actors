﻿// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Environment
    {
        internal IAddress Address { get; }
        internal List<Actor> Children { get; }
        internal IAddress? CompletesEventuallyAddress { get; private set; }
        internal Definition Definition { get; }
        internal FailureMark FailureMark { get; }

        internal ILogger Logger { get; }
        internal IMailbox Mailbox { get; }
        internal ISupervisor? MaybeSupervisor { get; }
        internal Actor? Parent { get; }
        private IDictionary<Type, object> ProxyCache { get; }
        internal Stage Stage { get; }
        internal Stowage Stowage { get; }
        internal Stowage Suspended { get; }

        private readonly AtomicBoolean secured;
        private readonly AtomicBoolean stopped;
        private Type[]? stowageOverrides;

        protected internal Environment(
            Stage stage,
            IAddress address,
            Definition definition,
            Actor? parent,
            IMailbox mailbox,
            ISupervisor? maybeSupervisor,
            ILogger logger)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (address.Id != World.PrivateRootId)
            {
                Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }
            else
            {
                Parent = parent;
            }
            Mailbox = mailbox ?? throw new ArgumentNullException(nameof(mailbox));
            MaybeSupervisor = maybeSupervisor;
            FailureMark = new FailureMark();
            Logger = logger;
            Children = new List<Actor>();
            ProxyCache = new Dictionary<Type, object>();
            Stowage = new Stowage();
            stowageOverrides = null;
            Suspended = new Stowage();

            secured = new AtomicBoolean(false);
            stopped = new AtomicBoolean(false);
        }

        internal void AddChild(Actor child)
        {
            Children.Add(child);
        }

        internal ICompletesEventually CompletesEventually(ResultCompletes completes)
        {
            if (CompletesEventuallyAddress == null)
            {
                var completesEventually = Stage.World.CompletesFor(completes.ClientCompletes());
                CompletesEventuallyAddress = completesEventually.Address;
                return completesEventually;
            }

            return Stage.World.CompletesFor(CompletesEventuallyAddress, completes.ClientCompletes());
        }

        internal void CacheProxy<T>(T proxy)
        {
            if (proxy != null) ProxyCache[proxy.GetType()] = proxy;
        }

        internal void CacheProxy(Type proxyType, object proxy)
        {
            ProxyCache[proxyType] = proxy;
        }

        internal T LookUpProxy<T>()
        {
            var lookup = LookUpProxy(typeof(T));
            if (lookup != null && !lookup.Equals(null))
            {
                return (T) lookup;
            }

            return default!;
        }

        internal object? LookUpProxy(Type type)
            => ProxyCache.ContainsKey(type) ? ProxyCache[type] : null;

        internal bool IsSecured => secured.Get();

        internal void SetSecured()
        {
            secured.Set(true);
        }

        internal bool IsStopped => stopped.Get();

        internal void Stop()
        {
            if (stopped.CompareAndSet(false, true))
            {
                StopChildren();
                Suspended.Reset();
                Stowage.Reset();
                Mailbox.Close();
            }
        }

        internal bool IsStowageOverride(Type protocol)
        {
            if (stowageOverrides != null)
            {
                return stowageOverrides.Contains(protocol);
            }

            return false;
        }

        internal void StowageOverrides(params Type[] overrides)
        {
            stowageOverrides = overrides;
        }

        private void StopChildren()
        {
            Children.ForEach(c => c.Stop());
            Children.Clear();
        }
    }
}
