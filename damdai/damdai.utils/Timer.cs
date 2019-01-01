using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Damdai
{
    [Serializable]
    [TypeConverter(typeof(Timer.Converter))]
    public class Timer
    {
        TimeSpan timespan_;
        TimeSpan initialTimespan_;
        [NonSerialized] DateTime lastUpdate_;

        public TimeSpan Length
        {
            get { return this.timespan_; }
            set
            {
                this.timespan_ = this.initialTimespan_ = value;
                this.lastUpdate_ = DateTime.Now;
            }
        }
        public bool CanUpdate
        {
            get { return this.timespan_.Ticks > 0; }
        }

        public Timer()
        {
        }
        public Timer(TimeSpan timespan)
        {
            this.initialTimespan_ = this.timespan_ = timespan;
            this.lastUpdate_ = DateTime.Now;
        }
        public Timer(int days, int hours, int minutes, int seconds)
            : this(new TimeSpan(days, hours, minutes, seconds))
        {
        }
        public Timer(Timer other) : this(other.initialTimespan_) { }

        public bool Update()
        {
            lock (this)
            {
                if (this.timespan_.Ticks > 0)
                {
                    DateTime now = DateTime.Now;
                    this.timespan_ -= (now - this.lastUpdate_);
                    this.lastUpdate_ = now;
                    if (this.timespan_.Ticks > 0)
                        return true;
                    else
                        return false;
                    //return (this.timespan_.Ticks > 0);
                }
                else
                    return false;
            }
        }
        public void Reset()
        {
            lock (this)
            {
                this.timespan_ = this.initialTimespan_;
                this.lastUpdate_ = DateTime.Now;
            }
        }

#if DEBUG
        class Converter : ExpandableObjectConverter
#else
        class Converter : TypeConverter
#endif
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                Timer timer = value as Timer;
                if (timer == null)
                {
                    return TimeSpan.Zero.ToString();
                }
                else
                {
                    return timer.initialTimespan_.ToString();
                }
            }
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value == null)
                    return new Timer();
                else
                {
                    TimeSpan timespan = TimeSpan.Parse((string)value);
                    if (timespan == TimeSpan.Zero)
                        return null;
                    else
                        return new Timer(timespan);
                }
            }
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return (sourceType == typeof(string));
            }
        }
    }
}
