﻿// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Common;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class FailureControlActor : Actor, IFailureControl
    {
        public static ThreadLocal<FailureControlActor> Instance = new ThreadLocal<FailureControlActor>();
        private readonly FailureControlTestResults testResults;

        public FailureControlActor(FailureControlTestResults testResults)
        {
            this.testResults = testResults;
            Instance.Value = this;
            testResults.Access = testResults.AfterCompleting(0);
        }

        public void AfterFailure()
        {
            testResults.Access.WriteUsing("afterFailureCount", 1);
        }

        public void AfterFailureCount(int count)
        {
            testResults.Access.WriteUsing("afterFailureCountCount", 1);
        }

        public void FailNow()
        {
            testResults.Access.WriteUsing("failNowCount", 1);
            throw new ApplicationException("Intended failure.");
        }

        protected internal override void BeforeStart()
        {
            testResults.Access.WriteUsing("beforeStartCount", 1);
            base.BeforeStart();
        }

        protected internal override void AfterStop()
        {
            testResults.Access.WriteUsing("afterStopCount", 1);
            base.AfterStop();
        }

        protected internal override void BeforeRestart(Exception reason)
        {
            testResults.Access.WriteUsing("beforeRestartCount", 1);
            base.BeforeRestart(reason);
        }

        protected internal override void AfterRestart(Exception reason)
        {
            base.AfterRestart(reason);
            testResults.Access.WriteUsing("afterRestartCount", 1);
        }

        protected internal override void BeforeResume(Exception reason)
        {
            testResults.Access.WriteUsing("beforeResume", 1);
            base.BeforeResume(reason);
        }

        public override void Stop()
        {
            testResults.Access.WriteUsing("stoppedCount", 1);
            base.Stop();
        }

        public class FailureControlTestResults
        {
            public AccessSafely Access { get; internal set; }
            public AtomicInteger AfterFailureCount = new AtomicInteger(0);
            public AtomicInteger AfterFailureCountCount = new AtomicInteger(0);
            public AtomicInteger AfterRestartCount = new AtomicInteger(0);
            public AtomicInteger AfterStopCount = new AtomicInteger(0);
            public AtomicInteger BeforeRestartCount = new AtomicInteger(0);
            public AtomicInteger BeforeResume = new AtomicInteger(0);
            public AtomicInteger BeforeStartCount = new AtomicInteger(0);
            public AtomicInteger FailNowCount = new AtomicInteger(0);
            public AtomicInteger StoppedCount = new AtomicInteger(0);

            public TestUntil UntilAfterFail = TestUntil.Happenings(0);
            public TestUntil UntilAfterRestart = TestUntil.Happenings(0);
            public TestUntil UntilBeforeResume = TestUntil.Happenings(0);
            public TestUntil UntilFailNow = TestUntil.Happenings(0);
            public TestUntil UntilFailureCount = TestUntil.Happenings(0);
            public TestUntil UntilStopped = TestUntil.Happenings(0);

            public FailureControlTestResults()
            {
                Access = AfterCompleting(0);
            }

            public AccessSafely AfterCompleting(int times)
            {
                Access = AccessSafely
                    .AfterCompleting(times)

                    .WritingWith("afterFailureCount", (int increment) => AfterFailureCount.Set(AfterFailureCount.Get() + increment))
                    .ReadingWith("afterFailureCount", () => AfterFailureCount.Get())

                    .WritingWith("afterFailureCountCount", (int increment) => AfterFailureCountCount.Set(AfterFailureCountCount.Get() + increment))
                    .ReadingWith("afterFailureCountCount", () => AfterFailureCountCount.Get())

                    .WritingWith("afterRestartCount", (int increment) => AfterRestartCount.Set(AfterRestartCount.Get() + increment))
                    .ReadingWith("afterRestartCount", () => AfterRestartCount.Get())

                    .WritingWith("afterStopCount", (int increment) => AfterStopCount.Set(AfterStopCount.Get() + increment))
                    .ReadingWith("afterStopCount", () => AfterStopCount.Get())

                    .WritingWith("beforeRestartCount", (int increment) => BeforeRestartCount.Set(BeforeRestartCount.Get() + increment))
                    .ReadingWith("beforeRestartCount", () => BeforeRestartCount.Get())

                    .WritingWith("beforeResume", (int increment) => BeforeResume.Set(BeforeResume.Get() + increment))
                    .ReadingWith("beforeResume", () => BeforeResume.Get())

                    .WritingWith("beforeStartCount", (int increment) => BeforeStartCount.Set(BeforeStartCount.Get() + increment))
                    .ReadingWith("beforeStartCount", () => BeforeStartCount.Get())

                    .WritingWith("failNowCount", (int increment) => FailNowCount.Set(FailNowCount.Get() + increment))
                    .ReadingWith("failNowCount", () => FailNowCount.Get())

                    .WritingWith("stoppedCount", (int increment) => StoppedCount.Set(StoppedCount.Get() + increment))
                    .ReadingWith("stoppedCount", () => StoppedCount.Get());

                return Access;
            }
        }
    }
}
