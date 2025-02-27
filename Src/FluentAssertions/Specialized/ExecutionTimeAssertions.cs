﻿using System;
using System.ComponentModel;
using FluentAssertions.Execution;

namespace FluentAssertions.Specialized
{
    /// <summary>
    /// Provides methods for asserting that the execution time of an <see cref="Action"/> satisfies certain conditions.
    /// </summary>
    public class ExecutionTimeAssertions
    {
        private readonly ExecutionTime execution;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionTime"/> class.
        /// </summary>
        /// <param name="executionTime">The execution on which time must be asserted.</param>
        public ExecutionTimeAssertions(ExecutionTime executionTime)
        {
            execution = executionTime ?? throw new ArgumentNullException(nameof(executionTime));
        }

        /// <summary>
        /// Checks the executing action if it satisfies a condition.
        /// If the execution runs into an exception, then this will rethrow it.
        /// </summary>
        /// <param name="condition">Condition to check on the current elapsed time.</param>
        /// <param name="expectedResult">Polling stops when condition returns the expected result.</param>
        /// <param name="rate">The rate at which the condition is re-checked.</param>
        /// <return>The elapsed time. (use this, don't measure twice)</return>
        private (bool isRunning, TimeSpan elapsed) PollUntil(Func<TimeSpan, bool> condition, bool expectedResult, TimeSpan rate)
        {
            TimeSpan elapsed = execution.ElapsedTime;
            bool isRunning = execution.IsRunning;

            while (isRunning)
            {
                if (condition(elapsed) == expectedResult)
                {
                    break;
                }

                isRunning = !execution.Task.Wait(rate);
                elapsed = execution.ElapsedTime;
            }

            if (execution.Exception is not null)
            {
                // rethrow captured exception
                throw execution.Exception;
            }

            return (isRunning, elapsed);
        }

        /// <summary>
        /// Asserts that the execution time of the operation is less than or equal to a specified amount of time.
        /// </summary>
        /// <param name="maxDuration">
        /// The maximum allowed duration.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<ExecutionTimeAssertions> BeLessThanOrEqualTo(TimeSpan maxDuration, string because = "", params object[] becauseArgs)
        {
            bool Condition(TimeSpan duration) => duration.CompareTo(maxDuration) <= 0;
            (bool isRunning, TimeSpan elapsed) = PollUntil(Condition, expectedResult: false, rate: maxDuration);

            Execute.Assertion
                .ForCondition(Condition(elapsed))
                .BecauseOf(because, becauseArgs)
                .FailWith("Execution of " +
                          execution.ActionDescription + " should be less than or equal to {0}{reason}, but it required " +
                          (isRunning ? "more than " : "exactly ") + "{1}.",
                    maxDuration,
                    elapsed);

            return new AndConstraint<ExecutionTimeAssertions>(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public AndConstraint<ExecutionTimeAssertions> BeLessOrEqualTo(TimeSpan maxDuration, string because = "", params object[] becauseArgs) => BeLessThanOrEqualTo(maxDuration, because, becauseArgs);

        /// <summary>
        /// Asserts that the execution time of the operation is less than a specified amount of time.
        /// </summary>
        /// <param name="maxDuration">
        /// The maximum allowed duration.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<ExecutionTimeAssertions> BeLessThan(TimeSpan maxDuration, string because = "", params object[] becauseArgs)
        {
            bool Condition(TimeSpan duration) => duration.CompareTo(maxDuration) < 0;
            (bool isRunning, TimeSpan elapsed) = PollUntil(Condition, expectedResult: false, rate: maxDuration);

            Execute.Assertion
                .ForCondition(Condition(execution.ElapsedTime))
                .BecauseOf(because, becauseArgs)
                .FailWith("Execution of " +
                          execution.ActionDescription + " should be less than {0}{reason}, but it required " +
                          (isRunning ? "more than " : "exactly ") + "{1}.",
                    maxDuration,
                    elapsed);

            return new AndConstraint<ExecutionTimeAssertions>(this);
        }

        /// <summary>
        /// Asserts that the execution time of the operation is greater than or equal to a specified amount of time.
        /// </summary>
        /// <param name="minDuration">
        /// The minimum allowed duration.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<ExecutionTimeAssertions> BeGreaterThanOrEqualTo(TimeSpan minDuration, string because = "", params object[] becauseArgs)
        {
            bool Condition(TimeSpan duration) => duration.CompareTo(minDuration) >= 0;
            (bool isRunning, TimeSpan elapsed) = PollUntil(Condition, expectedResult: true, rate: minDuration);

            Execute.Assertion
                .ForCondition(Condition(elapsed))
                .BecauseOf(because, becauseArgs)
                .FailWith("Execution of " +
                          execution.ActionDescription + " should be greater than or equal to {0}{reason}, but it required " +
                          (isRunning ? "more than " : "exactly ") + "{1}.",
                    minDuration,
                    elapsed);

            return new AndConstraint<ExecutionTimeAssertions>(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public AndConstraint<ExecutionTimeAssertions> BeGreaterOrEqualTo(TimeSpan minDuration, string because = "", params object[] becauseArgs) => BeGreaterThanOrEqualTo(minDuration, because, becauseArgs);

        /// <summary>
        /// Asserts that the execution time of the operation is greater than a specified amount of time.
        /// </summary>
        /// <param name="minDuration">
        /// The minimum allowed duration.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<ExecutionTimeAssertions> BeGreaterThan(TimeSpan minDuration, string because = "", params object[] becauseArgs)
        {
            bool Condition(TimeSpan duration) => duration.CompareTo(minDuration) > 0;
            (bool isRunning, TimeSpan elapsed) = PollUntil(Condition, expectedResult: true, rate: minDuration);

            Execute.Assertion
                .ForCondition(Condition(elapsed))
                .BecauseOf(because, becauseArgs)
                .FailWith("Execution of " +
                          execution.ActionDescription + " should be greater than {0}{reason}, but it required " +
                          (isRunning ? "more than " : "exactly ") + "{1}.",
                    minDuration,
                    elapsed);

            return new AndConstraint<ExecutionTimeAssertions>(this);
        }

        /// <summary>
        /// Asserts that the execution time of the operation is within the expected duration.
        /// by a specified precision.
        /// </summary>
        /// <param name="expectedDuration">
        /// The expected duration.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of time which the execution time may differ from the expected duration.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<ExecutionTimeAssertions> BeCloseTo(TimeSpan expectedDuration, TimeSpan precision, string because = "", params object[] becauseArgs)
        {
            if (precision < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(precision), $"The value of {nameof(precision)} must be non-negative.");
            }

            TimeSpan minimumValue = expectedDuration - precision;
            TimeSpan maximumValue = expectedDuration + precision;

            bool MaxCondition(TimeSpan duration) => duration <= maximumValue;
            bool MinCondition(TimeSpan duration) => duration >= minimumValue;

            // for polling we only use max condition, we don't want poll to stop if
            // elapsed time didn't even get to the acceptable range
            (bool isRunning, TimeSpan elapsed) = PollUntil(MaxCondition, expectedResult: false, rate: maximumValue);

            Execute.Assertion
                .ForCondition(MinCondition(elapsed) && MaxCondition(elapsed))
                .BecauseOf(because, becauseArgs)
                .FailWith("Execution of " + execution.ActionDescription +
                          " should be within {0} from {1}{reason}, but it required " +
                          (isRunning ? "more than " : "exactly ") + "{2}.",
                    precision,
                    expectedDuration,
                    elapsed);

            return new AndConstraint<ExecutionTimeAssertions>(this);
        }
    }
}
