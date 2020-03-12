﻿// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors
{
    internal sealed class BasicAddressFactory : IAddressFactory
    {
        private static readonly IAddress None = new BasicAddress(0, "(none)");
        private readonly AtomicLong highId;
        private readonly AtomicLong nextId;

        internal BasicAddressFactory()
        {
            highId = new AtomicLong(World.HighRootId);
            nextId = new AtomicLong(1);
        }

        public IAddress FindableBy<T>(T id) => new BasicAddress(long.Parse(id!.ToString()));

        public IAddress From(long reservedId, string name) => new BasicAddress(reservedId, name);

        public IAddress From(string idString) => new BasicAddress(long.Parse(idString));

        public IAddress From(string idString, string name) => new BasicAddress(long.Parse(idString), name);

        public long TestNextIdValue() => nextId.Get(); // for test only

        public IAddress Unique() => new BasicAddress(nextId.GetAndIncrement());

        public IAddress UniquePrefixedWith(string prefixedWith) => new BasicAddress(nextId.GetAndIncrement(), prefixedWith, true);

        public IAddress UniqueWith(string? name) => new BasicAddress(nextId.GetAndIncrement(), name);

        public IAddress WithHighId() => WithHighId(null);

        public IAddress WithHighId(string? name) => new BasicAddress(highId.DecrementAndGet(), name);

        IAddress IAddressFactory.None() => None;
    }
}
