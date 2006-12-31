/* 
* Copyright 2004-2005 OpenSymphony 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); you may not 
* use this file except in compliance with the License. You may obtain a copy 
* of the License at 
* 
*   http://www.apache.org/licenses/LICENSE-2.0 
*   
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
* WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
* License for the specific language governing permissions and limitations 
* under the License.
* 
*/

/*
* Previously Copyright (c) 2001-2004 James House
*/
using System;
using System.Collections;

using Nullables;

namespace Quartz
{
	/// <summary> 
	/// A concrete <code>Trigger</code> that is used to fire a <code>JobDetail</code>
	/// at a given moment in time, and optionally repeated at a specified interval.
	/// </summary>
	/// <seealso cref="Trigger" />
	/// <seealso cref="CronTrigger" />
	/// <seealso cref="TriggerUtils" />
	/// 
	/// <author>James House</author>
	/// <author>Contributions by Lieven Govaerts of Ebitec Nv, Belgium.</author>
	/// <author>Marko Lahma (.NET)</author>
	[Serializable]
	public class SimpleTrigger : Trigger
	{


		/// <summary>
		/// Get or set thhe number of times the <code>SimpleTrigger</code> should
		/// repeat, after which it will be automatically deleted.
		/// </summary>
		/// <seealso cref="REPEAT_INDEFINITELY" />
		public int RepeatCount
		{
			get { return repeatCount; }

			set
			{
				if (value < 0 && value != REPEAT_INDEFINITELY)
				{
					throw new ArgumentException("Repeat count must be >= 0, use the " + "constant REPEAT_INDEFINITELY for infinite.");
				}

				repeatCount = value;
			}
		}

		/// <summary>
		/// Get or set the the time interval (in milliseconds) at which the <code>SimpleTrigger</code>
		/// should repeat.
		/// </summary>
		public long RepeatInterval
		{
			get { return repeatInterval; }

			set
			{
				if (value < 0)
				{
					throw new ArgumentException("Repeat interval must be >= 0");
				}

				repeatInterval = value;
			}
		}

		/// <summary>
		/// Get or set the number of times the <code>SimpleTrigger</code> has already
		/// fired.
		/// </summary>
		public virtual int TimesTriggered
		{
			get { return timesTriggered; }
			set { timesTriggered = value; }
		}

		/// <summary> 
		/// Returns the final time at which the <code>SimpleTrigger</code> will
		/// fire, if repeatCount is REPEAT_INDEFINITELY, null will be returned.
		/// <p>
		/// Note that the return time may be in the past.
		/// </p>
		/// </summary>
		public override NullableDateTime FinalFireTime
		{
			get
			{
				if (repeatCount == 0)
				{
					return StartTime;
				}

				if (repeatCount == REPEAT_INDEFINITELY && !EndTime.HasValue)
				{
					return NullableDateTime.Default;
				}

				if (repeatCount == REPEAT_INDEFINITELY && !EndTime.HasValue)
				{
					return NullableDateTime.Default;
				}
				else if (repeatCount == REPEAT_INDEFINITELY)
				{
					return GetFireTimeBefore(EndTime);
				}

				DateTime lastTrigger = StartTime.AddMilliseconds(repeatCount*repeatInterval);

				if (!EndTime.HasValue || lastTrigger < EndTime.Value)
				{
					return lastTrigger;
				}
				else
				{
					return GetFireTimeBefore(EndTime);
				}
			}
		}

		/// <summary>
		/// Tells whether this Trigger instance can handle events
		/// in millisecond precision.
		/// </summary>
		/// <value></value>
		public override bool HasMillisecondPrecision
		{
			get { return true; }
		}

		/// <summary> 
		/// Instructs the <code>{@link Scheduler}</code> that upon a mis-fire
		/// situation, the <code>{@link SimpleTrigger}</code> wants to be fired
		/// now by <code>Scheduler</code>.
		/// <p>
		/// <i>NOTE:</i> This instruction should typically only be used for
		/// 'one-shot' (non-repeating) Triggers. If it is used on a trigger with a
		/// repeat count > 0 then it is equivalent to the instruction <code>{@link #MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_REMAINING_REPEAT_COUNT}
		/// </code>.
		/// </p>
		/// </summary>
		public const int MISFIRE_INSTRUCTION_FIRE_NOW = 1;

		/// <summary> <p>
		/// Instructs the <code>{@link Scheduler}</code> that upon a mis-fire
		/// situation, the <code>{@link SimpleTrigger}</code> wants to be
		/// re-scheduled to 'now' (even if the associated <code>{@link Calendar}</code>
		/// excludes 'now') with the repeat count left as-is.
		/// </p>
		/// 
		/// <p>
		/// <i>NOTE:</i> Use of this instruction causes the trigger to 'forget'
		/// the start-time and repeat-count that it was originally setup with (this
		/// is only an issue if you for some reason wanted to be able to tell what
		/// the original values were at some later time).
		/// </p>
		/// 
		/// <p>
		/// <i>NOTE:</i> This instruction could cause the <code>Trigger</code>
		/// to go to the 'COMPLETE' state after firing 'now', if all the
		/// repeat-fire-times where missed.
		/// </p>
		/// </summary>
		public const int MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_EXISTING_REPEAT_COUNT = 2;

		/// <summary> <p>
		/// Instructs the <code>{@link Scheduler}</code> that upon a mis-fire
		/// situation, the <code>{@link SimpleTrigger}</code> wants to be
		/// re-scheduled to 'now' (even if the associated <code>{@link Calendar}</code>
		/// excludes 'now') with the repeat count set to what it would be, if it had
		/// not missed any firings.
		/// </p>
		/// 
		/// <p>
		/// <i>NOTE:</i> Use of this instruction causes the trigger to 'forget'
		/// the start-time and repeat-count that it was originally setup with (this
		/// is only an issue if you for some reason wanted to be able to tell what
		/// the original values were at some later time).
		/// </p>
		/// 
		/// <p>
		/// <i>NOTE:</i> This instruction could cause the <code>Trigger</code>
		/// to go to the 'COMPLETE' state after firing 'now', if all the
		/// repeat-fire-times where missed.
		/// </p>
		/// </summary>
		public const int MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_REMAINING_REPEAT_COUNT = 3;

		/// <summary> <p>
		/// Instructs the <code>{@link Scheduler}</code> that upon a mis-fire
		/// situation, the <code>{@link SimpleTrigger}</code> wants to be
		/// re-scheduled to the next scheduled time after 'now' - taking into
		/// account any associated <code>{@link Calendar}</code>, and with the
		/// repeat count set to what it would be, if it had not missed any firings.
		/// </p>
		/// 
		/// <p>
		/// <i>NOTE/WARNING:</i> This instruction could cause the <code>Trigger</code>
		/// to go directly to the 'COMPLETE' state if all fire-times where missed.
		/// </p>
		/// </summary>
		public const int MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_REMAINING_COUNT = 4;

		/// <summary>
		/// Instructs the <code>{@link Scheduler}</code> that upon a mis-fire
		/// situation, the <code>{@link SimpleTrigger}</code> wants to be
		/// re-scheduled to the next scheduled time after 'now' - taking into
		/// account any associated <code>{@link Calendar}</code>, and with the
		/// repeat count left unchanged.
		/// <p>
		/// <i>NOTE:</i> Use of this instruction causes the trigger to 'forget'
		/// the repeat-count that it was originally setup with (this is only an
		/// issue if you for some reason wanted to be able to tell what the original
		/// values were at some later time).
		/// </p>
		/// <p>
		/// <i>NOTE/WARNING:</i> This instruction could cause the <code>Trigger</code>
		/// to go directly to the 'COMPLETE' state if all fire-times where missed.
		/// </p>
		/// </summary>
		public const int MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_EXISTING_COUNT = 5;

		/// <summary>
		/// Used to indicate the 'repeat count' of the trigger is indefinite. Or in
		/// other words, the trigger should repeat continually until the trigger's
		/// ending timestamp.
		/// </summary>
		public const int REPEAT_INDEFINITELY = -1;

		private NullableDateTime nextFireTime = null;
		private NullableDateTime previousFireTime = null;

		private int repeatCount = 0;
		private long repeatInterval = 0;
		private int timesTriggered = 0;
		private bool complete = false;

		/// <summary>
		/// Create a <code>SimpleTrigger</code> with no settings.
		/// </summary>
		public SimpleTrigger() : base()
		{
		}

		/// <summary>
		/// Create a <code>SimpleTrigger</code> that will occur immediately, and
		/// not repeat.
		/// </summary>
		public SimpleTrigger(string name, string group) : this(name, group, DateTime.Now, null, 0, 0)
		{
		}

		/// <summary>
		/// Create a <code>SimpleTrigger</code> that will occur immediately, and
		/// repeat at the the given interval the given number of times.
		/// </summary>
		public SimpleTrigger(string name, string group, int repeatCount, long repeatInterval)
			: this(name, group, DateTime.Now, null, repeatCount, repeatInterval)
		{
		}

		/// <summary>
		/// Create a <code>SimpleTrigger</code> that will occur at the given time,
		/// and not repeat.
		/// </summary>
		public SimpleTrigger(string name, string group, DateTime startTime) : this(name, group, startTime, null, 0, 0)
		{
		}

		/// <summary> 
		/// Create a <code>SimpleTrigger</code> that will occur at the given time,
		/// and repeat at the the given interval the given number of times, or until
		/// the given end time.
		/// </summary>
		/// <param name="startTime">
		/// A <code>Date</code> set to the time for the <code>Trigger</code> to fire.
		/// </param>
		/// <param name="endTime">
		/// A <code>Date</code> set to the time for the <code>Trigger</code>
		/// to quit repeat firing.
		/// </param>
		/// <param name="repeatCount">
		/// The number of times for the <code>Trigger</code> to repeat
		/// firing, use {@link #REPEAT_INDEFINITELY}for unlimitted times.
		/// </param>
		/// <param name="repeatInterval">
		/// The number of milliseconds to pause between the repeat firing.
		/// </param>
		public SimpleTrigger(string name, string group, DateTime startTime, NullableDateTime endTime, int repeatCount,
		                     long repeatInterval) : base(name, group)
		{
			StartTime = startTime;
			EndTime = endTime;
			RepeatCount = repeatCount;
			RepeatInterval = repeatInterval;
		}

		/// <summary> 
		/// Create a <code>SimpleTrigger</code> that will occur at the given time,
		/// fire the identified <code>Job</code> and repeat at the the given
		/// interval the given number of times, or until the given end time.
		/// </summary>
		/// <param name="startTime">
		/// A <code>Date</code> set to the time for the <code>Trigger</code>
		/// to fire.
		/// </param>
		/// <param name="endTime">
		/// A <code>Date</code> set to the time for the <code>Trigger</code>
		/// to quit repeat firing.
		/// </param>
		/// <param name="repeatCount">
		/// The number of times for the <code>Trigger</code> to repeat
		/// firing, use REPEAT_INDEFINITELY for unlimitted times.
		/// </param>
		/// <param name="repeatInterval">
		/// The number of milliseconds to pause between the repeat firing.
		/// </param>
		public SimpleTrigger(string name, string group, string jobName, string jobGroup, DateTime startTime,
		                     NullableDateTime endTime, int repeatCount, long repeatInterval)
			: base(name, group, jobName, jobGroup)
		{
			StartTime = startTime;
			EndTime = endTime;
			RepeatCount = repeatCount;
			RepeatInterval = repeatInterval;
		}

		protected override bool ValidateMisfireInstruction(int misfireInstruction)
		{
			if (misfireInstruction < MISFIRE_INSTRUCTION_SMART_POLICY)
			{
				return false;
			}

			if (misfireInstruction > MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_EXISTING_COUNT)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Updates the <code>SimpleTrigger</code>'s state based on the
		/// MISFIRE_INSTRUCTION_XXX that was selected when the <code>SimpleTrigger</code>
		/// was created.
		/// <p>
		/// If the misfire instruction is set to MISFIRE_INSTRUCTION_SMART_POLICY,
		/// then the following scheme will be used: <br />
		/// <ul>
		/// <li>If the Repeat Count is <code>0</code>, then the instruction will
		/// be interpreted as <code>MISFIRE_INSTRUCTION_FIRE_NOW</code>.</li>
		/// <li>If the Repeat Count is <code>REPEAT_INDEFINITELY</code>, then
		/// the instruction will be interpreted as <code>MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_REMAINING_COUNT</code>.
		/// <b>WARNING:</b> using MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_REMAINING_COUNT 
		/// with a trigger that has a non-null end-time may cause the trigger to 
		/// never fire again if the end-time arrived during the misfire time span. 
		/// </li>
		/// <li>If the Repeat Count is <code>> 0</code>, then the instruction
		/// will be interpreted as <code>MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_EXISTING_REPEAT_COUNT</code>.
		/// </li>
		/// </ul>
		/// </p>
		/// </summary>
		public override void UpdateAfterMisfire(ICalendar cal)
		{
			int instr = MisfireInstruction;
			if (instr == MISFIRE_INSTRUCTION_SMART_POLICY)
			{
				if (RepeatCount == 0)
				{
					instr = MISFIRE_INSTRUCTION_FIRE_NOW;
				}
				else if (RepeatCount == REPEAT_INDEFINITELY)
				{
					instr = MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_REMAINING_COUNT;
				}
				else
				{
					// if (getRepeatCount() > 0)
					instr = MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_EXISTING_REPEAT_COUNT;
				}
			}
			else if (instr == MISFIRE_INSTRUCTION_FIRE_NOW && RepeatCount != 0)
			{
				instr = MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_REMAINING_REPEAT_COUNT;
			}

			if (instr == MISFIRE_INSTRUCTION_FIRE_NOW)
			{
				SetNextFireTime(DateTime.Now);
			}
			else if (instr == MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_EXISTING_COUNT)
			{
				NullableDateTime newFireTime = GetFireTimeAfter(DateTime.Now);

				while (newFireTime.HasValue && cal != null && !cal.IsTimeIncluded(newFireTime.Value))
				{
					newFireTime = GetFireTimeAfter(newFireTime);
				}
				SetNextFireTime(newFireTime);
			}
			else if (instr == MISFIRE_INSTRUCTION_RESCHEDULE_NEXT_WITH_REMAINING_COUNT)
			{
				NullableDateTime newFireTime = GetFireTimeAfter(DateTime.Now);

				while (newFireTime.HasValue && cal != null && !cal.IsTimeIncluded(newFireTime.Value))
				{
					newFireTime = GetFireTimeAfter(newFireTime);
				}

				if (newFireTime.HasValue)
				{
					int timesMissed = ComputeNumTimesFiredBetween(nextFireTime, newFireTime);
					TimesTriggered = TimesTriggered + timesMissed;
				}

				SetNextFireTime(newFireTime);
			}
			else if (instr == MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_EXISTING_REPEAT_COUNT)
			{
				DateTime newFireTime = DateTime.Now;
				if (repeatCount != 0 && repeatCount != REPEAT_INDEFINITELY)
				{
					RepeatCount = RepeatCount - TimesTriggered;
					TimesTriggered = 0;
				}
				StartTime = newFireTime;
				SetNextFireTime(newFireTime);
			}
			else if (instr == MISFIRE_INSTRUCTION_RESCHEDULE_NOW_WITH_REMAINING_REPEAT_COUNT)
			{
				DateTime newFireTime = DateTime.Now;
				int timesMissed = ComputeNumTimesFiredBetween(nextFireTime, newFireTime);

				if (repeatCount != 0 && repeatCount != REPEAT_INDEFINITELY)
				{
					int remainingCount = RepeatCount - (TimesTriggered + timesMissed);
					if (remainingCount <= 0)
					{
						remainingCount = 0;
					}
					RepeatCount = remainingCount;
					TimesTriggered = 0;
				}

				if (EndTime.HasValue && EndTime.Value < newFireTime)
				{
					EndTime = newFireTime.AddMilliseconds(50);
				}

				StartTime = newFireTime;
				SetNextFireTime(newFireTime);
			}
		}

		/// <summary>
		/// Called when the <code>{@link Scheduler}</code> has decided to 'fire'
		/// the trigger (Execute the associated <code>Job</code>), in order to
		/// give the <code>Trigger</code> a chance to update itself for its next
		/// triggering (if any).
		/// </summary>
		/// <seealso cref="JobExecutionException" />
		public override void Triggered(ICalendar cal)
		{
			timesTriggered++;
			previousFireTime = nextFireTime;
			nextFireTime = GetFireTimeAfter(nextFireTime);

			while (nextFireTime.HasValue && cal != null && !cal.IsTimeIncluded(nextFireTime.Value))
			{
				nextFireTime = GetFireTimeAfter(nextFireTime);
			}
		}


		public override void UpdateWithNewCalendar(ICalendar calendar, long misfireThreshold)
		{
			nextFireTime = GetFireTimeAfter(previousFireTime);

			DateTime now = DateTime.Now;
			do
			{
				while (nextFireTime.HasValue && calendar != null && !calendar.IsTimeIncluded(nextFireTime.Value))
				{
					nextFireTime = GetFireTimeAfter(nextFireTime);
				}

				if (nextFireTime.HasValue && nextFireTime.Value < now)
				{
					long diff = (long) (now - nextFireTime.Value).TotalMilliseconds;
					if (diff >= misfireThreshold)
					{
						nextFireTime = GetFireTimeAfter(nextFireTime);
						continue;
					}
				}
			} while (false);
		}

		/// <summary>
		/// Called by the scheduler at the time a <code>Trigger</code> is first
		/// added to the scheduler, in order to have the <code>Trigger</code>
		/// compute its first fire time, based on any associated calendar.
		/// <p>
		/// After this method has been called, <code>getNextFireTime()</code>
		/// should return a valid answer.
		/// </p>
		/// </summary>
		/// <returns> 
		/// The first time at which the <code>Trigger</code> will be fired
		/// by the scheduler, which is also the same value <code>getNextFireTime()</code>
		/// will return (until after the first firing of the <code>Trigger</code>).
		/// </returns>
		public override NullableDateTime ComputeFirstFireTime(ICalendar cal)
		{
			nextFireTime = StartTime;

			while (nextFireTime.HasValue && cal != null && !cal.IsTimeIncluded(nextFireTime.Value))
			{
				nextFireTime = GetFireTimeAfter(nextFireTime);
			}

			return nextFireTime.Value;
		}

		/// <summary>
		/// Called after the <code>{@link Scheduler}</code> has executed the
		/// <code>{@link org.quartz.JobDetail}</code> associated with the <code>Trigger</code>
		/// in order to get the final instruction code from the trigger.
		/// </summary>
		/// <param name="context">
		/// is the <code>JobExecutionContext</code> that was used by the
		/// <code>Job</code>'s<code>Execute(xx)</code> method.
		/// </param>
		/// <param name="result">
		/// is the <code>JobExecutionException</code> thrown by the
		/// <code>Job</code>, if any (may be null).
		/// </param>
		/// <returns> 
		/// One of the Trigger.INSTRUCTION_XXX constants.
		/// </returns>
		/// <seealso cref="Trigger.INSTRUCTION_NOOP" />
		/// <seealso cref="Trigger.INSTRUCTION_RE_EXECUTE_JOB" />
		/// <seealso cref="Trigger.INSTRUCTION_DELETE_TRIGGER" />
		/// <seealso cref="Trigger.INSTRUCTION_SET_TRIGGER_COMPLETE" />
		/// <seealso cref="Triggered(ICalendar)" />
		public override int ExecutionComplete(JobExecutionContext context, JobExecutionException result)
		{
			if (result != null && result.RefireImmediately())
			{
				return INSTRUCTION_RE_EXECUTE_JOB;
			}

			if (result != null && result.unscheduleFiringTrigger())
			{
				return INSTRUCTION_SET_TRIGGER_COMPLETE;
			}

			if (result != null && result.unscheduleAllTriggers())
			{
				return INSTRUCTION_SET_ALL_JOB_TRIGGERS_COMPLETE;
			}

			if (!MayFireAgain())
			{
				return INSTRUCTION_DELETE_TRIGGER;
			}

			return INSTRUCTION_NOOP;
		}

		/// <summary>
		/// Returns the next time at which the <code>SimpleTrigger</code> will
		/// fire. If the trigger will not fire again, <code>null</code> will be
		/// returned. The value returned is not guaranteed to be valid until after
		/// the <code>Trigger</code> has been added to the scheduler.
		/// </summary>
		public override NullableDateTime GetNextFireTime()
		{
			return nextFireTime;
		}

		/// <summary>
		/// Returns the previous time at which the <code>SimpleTrigger</code> will
		/// fire. If the trigger has not yet fired, <code>null</code> will be
		/// returned.
		/// </summary>
		public override NullableDateTime GetPreviousFireTime()
		{
			return previousFireTime;
		}

		/// <summary>
		/// Set the next time at which the <code>SimpleTrigger</code> should fire.
		/// <strong>This method should not be invoked by client code.</strong>
		/// </summary>
		public void SetNextFireTime(NullableDateTime fireTime)
		{
			nextFireTime = fireTime;
		}

		/// <summary>
		/// Set the previous time at which the <code>SimpleTrigger</code> fired.
		/// <strong>This method should not be invoked by client code.</strong>
		/// </summary>
		public virtual void SetPreviousFireTime(NullableDateTime fireTime)
		{
			previousFireTime = fireTime;
		}

		/// <summary> 
		/// Returns the next time at which the <code>SimpleTrigger</code> will
		/// fire, after the given time. If the trigger will not fire after the given
		/// time, <code>null</code> will be returned.
		/// </summary>
		public override NullableDateTime GetFireTimeAfter(NullableDateTime afterTime)
		{
			if (complete)
			{
				return NullableDateTime.Default;
			}

			if ((timesTriggered > repeatCount) && (repeatCount != REPEAT_INDEFINITELY))
			{
				return NullableDateTime.Default;
			}

			if (!afterTime.HasValue)
			{
				afterTime = DateTime.Now;
			}

			if (repeatCount == 0 && afterTime.CompareTo(StartTime) >= 0)
			{
				return NullableDateTime.Default;
			}

			DateTime startMillis = StartTime;
			DateTime afterMillis = afterTime.Value;
			DateTime endMillis = !EndTime.HasValue ? DateTime.MaxValue : EndTime.Value;

			if (endMillis <= afterMillis)
			{
				return NullableDateTime.Default;
			}

			if (startMillis < afterMillis && repeatCount == 0)
			{
				return NullableDateTime.Default;
			}

			if (afterMillis < startMillis)
			{
				return startMillis;
			}

			long numberoftimesexecutedplusone = (((long) (afterMillis - startMillis).TotalMilliseconds)/repeatInterval) + 1;

			if ((numberoftimesexecutedplusone > repeatCount) && (repeatCount != REPEAT_INDEFINITELY))
			{
				return NullableDateTime.Default;
			}

			DateTime time = startMillis.AddMilliseconds(numberoftimesexecutedplusone*repeatInterval);
			if (endMillis <= time)
			{
				return NullableDateTime.Default;
			}

			return time;
		}

		/// <summary>
		/// Returns the last time at which the <code>SimpleTrigger</code> will
		/// fire, before the given time. If the trigger will not fire before the
		/// given time, <code>null</code> will be returned.
		/// </summary>
		public virtual NullableDateTime GetFireTimeBefore(NullableDateTime end)
		{
			if (end.Value < StartTime)
			{
				return NullableDateTime.Default;
			}

			int numFires = ComputeNumTimesFiredBetween(StartTime, end);
			return StartTime.AddMilliseconds(numFires*repeatInterval);
		}

		public virtual int ComputeNumTimesFiredBetween(NullableDateTime start, NullableDateTime end)
		{
			long time = (long) (end.Value - start.Value).TotalMilliseconds;
			return (int) (time/repeatInterval);
		}

		/// <summary> 
		/// Determines whether or not the <code>SimpleTrigger</code> will occur
		/// again.
		/// </summary>
		public override bool MayFireAgain()
		{
			return GetNextFireTime().HasValue;
		}

		/// <summary>
		/// Validates whether the properties of the <code>JobDetail</code> are
		/// valid for submission into a <code>Scheduler</code>.
		/// </summary>
		public override void Validate()
		{
			base.Validate();

			if (repeatCount != 0 && repeatInterval < 1)
			{
				throw new SchedulerException("Repeat Interval cannot be zero.", SchedulerException.ERR_CLIENT_ERROR);
			}
		}

		[STAThread]
		public static void Main(string[] args)
		{
			DateTime sdt = DateTime.Now;
			DateTime edt = sdt.AddMilliseconds(55000);
			SimpleTrigger st = new SimpleTrigger("t", "g", "j", "g", sdt, edt, 10, 10000L);
			Console.Error.WriteLine();
			st.ComputeFirstFireTime(null);
			Console.Error.WriteLine("lastTime=" + st.FinalFireTime.Value.ToString("r"));

			IList times = TriggerUtils.ComputeFireTimes(st, null, 50);
			for (int i = 0; i < times.Count; i++)
			{
				Console.Error.WriteLine("firetime = " + times[i]);
			}
		}

	}
}