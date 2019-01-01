using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public class WeatherConditions : MarshalByRefObject, ICloneable
    {
        public enum Temperature
        {
            Freezing,
            Cold,
            Cool,
            Perfect,
            Warm,
            Hot,
            Scorching
        }

        public enum Conditions
        {
            Sunny,
            Cloudy,
            Drizzling,
            Raining,
        }

        public enum WindSpeeds
        {
            None,
            Calm,
            Breezy,
            Windy,
            ThunderStorm,
            TropicalStorm,
            Hurricane,
        }

        #region Members
        BitArray possibletemps_ = new BitArray(16);
        BitArray possibleconditions_ = new BitArray(16);
        BitArray possiblewinds_ = new BitArray(16);
        Temperature temp_ = Temperature.Perfect;
        Conditions condition_ = Conditions.Sunny;
        WindSpeeds wind_ = WindSpeeds.None;
        Temperature nexttemp_;
        Conditions nextcondition_;
        WindSpeeds nextwind_;
        #endregion

        #region Properties
        //[Editor(typeof(FlagEditor<Temperature>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<Temperature>))]
        public BitArray PossibleTemperatures
        {
            get { return this.possibletemps_; }
            set { this.possibletemps_ = value; }
        }

        //[Editor(typeof(FlagEditor<Conditions>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<Conditions>))]
        public BitArray PossibleConditions
        {
            get { return this.possibleconditions_; }
            set { this.possibleconditions_ = value; }
        }

        //[Editor(typeof(FlagEditor<WindSpeeds>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<WindSpeeds>))]
        [Browsable(false)]
        public BitArray PossibleWindSpeeds
        {
            get { return this.possiblewinds_; }
            set { this.possiblewinds_ = value; }
        }

        [Browsable(false)]
        public Temperature CurrentTemp
        {
            get { return this.temp_; }
            set { this.temp_ = value; }
        }

        [Browsable(false)]
        public Conditions CurrentCondition
        {
            get { return this.condition_; }
            set { this.condition_ = value; }
        }

        [Browsable(false)]
        public WindSpeeds CurrentWindSpeed
        {
            get { return this.wind_; }
            set { this.wind_ = value; }
        }

        [Browsable(false)]
        public Temperature NextTemp
        {
            get { return this.nexttemp_; }
            set { this.nexttemp_ = value; }
        }

        [Browsable(false)]
        public Conditions NextCondition
        {
            get { return this.nextcondition_; }
            set { this.nextcondition_ = value; }
        }

        [Browsable(false)]
        public WindSpeeds NextWindSpeed
        {
            get { return this.nextwind_; }
            set { this.nextwind_ = value; }
        }
        #endregion

        public override string ToString()
        {
            string temp = (Damdai.Collections.Util.IsEmpty(this.possibletemps_) ? "n/a" : this.CurrentTemp.ToString());
            string cond = (Damdai.Collections.Util.IsEmpty(this.possibleconditions_) ? "n/a" : this.CurrentCondition.ToString());
            return "Temp: " + temp + ", Condition: " + cond;// +", Wind: " + this.CurrentWindSpeed;
        }

        [Obfuscation(Exclude = false)]
        public void Update(IObject o)
        {
            int chance = 5;
            if (Globals.Random.Next(chance) == 1)
                this.UpdateTemp(o);
            if (Globals.Random.Next(chance) == 1)
                this.UpdateCondition(o);
            //if (Server.RandomNumber.Next(chance) == 1)
            //    this.UpdateWind(area);
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateTemp(IObject o)
        {
            List<Temperature> list = new List<Temperature>();
            for (int x = 0; x < this.PossibleTemperatures.Count; ++x)
                if (this.PossibleTemperatures.Get(x))
                    list.Add((Temperature)x);

            if (list.Count == 0)
                return;
            else if (list.Count == 1 || !list.Contains(this.CurrentTemp))
            {
                this.CurrentTemp = list[0];
                return;
            }

            if (this.CurrentTemp == this.NextTemp)
                this.NextTemp = list[Globals.Random.Next(list.Count)];

            if (this.NextTemp < this.CurrentTemp)
            {
                switch (--this.CurrentTemp)
                {
                    case Temperature.Freezing:
                        o.Send("Water transforms to ice as the air begins to freeze.");
                        break;
                    case Temperature.Cold:
                        o.Send("The air is getting colder.");
                        break;
                    case Temperature.Cool:
                        o.Send("You feel a cool breeze on your skin.");
                        break;
                    case Temperature.Perfect:
                        o.Send("The air has reached a perfect temperature.");
                        break;
                    case Temperature.Warm:
                        o.Send("The heat gives way to a mild warmth.");
                        break;
                    case Temperature.Hot:
                        o.Send("You feel relief as the scorching heat begins to let up.");
                        break;
                }
            }
            else if (this.NextTemp > this.CurrentTemp)
            {
                switch (++this.CurrentTemp)
                {
                    case Temperature.Cold:
                        o.Send("Ice begins to melt as the freezing temperature rises.");
                        break;
                    case Temperature.Cool:
                        o.Send("You feel slightly warmer as the cold air becomes cool.");
                        break;
                    case Temperature.Perfect:
                        o.Send("The air has reached a perfect temperature.");
                        break;
                    case Temperature.Warm:
                        o.Send("You feel a warm breeze on your skin.");
                        break;
                    case Temperature.Hot:
                        o.Send("The air is getting hotter.");
                        break;
                    case Temperature.Scorching:
                        o.Send("You begin sweating as the heat becomes unbearable.");
                        break;
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateCondition(IObject o)
        {
            List<Conditions> list = new List<Conditions>();
            for (int x = 0; x < this.PossibleConditions.Count; ++x)
                if (this.PossibleConditions.Get(x))
                    list.Add((Conditions)x);

            if (list.Count == 0)
                return;
            else if (list.Count == 1 || !list.Contains(this.CurrentCondition))
            {
                this.CurrentCondition = list[0];
                return;
            }

            if (this.CurrentCondition == this.NextCondition)
                this.NextCondition = list[Globals.Random.Next(list.Count)];

            if (this.NextCondition < this.CurrentCondition)
            {
                switch (--this.CurrentCondition)
                {
                    case Conditions.Sunny:
                        o.Send("You feel a soothing warmth as the sun shines down on your face.");
                        break;
                    case Conditions.Cloudy:
                        o.Send("It stops raining.");
                        break;
                    case Conditions.Drizzling:
                        o.Send("The rain has turned into a light drizzle.");
                        break;
                }
            }
            else if (this.NextCondition > this.CurrentCondition)
            {
                switch (++this.CurrentCondition)
                {
                    case Conditions.Cloudy:
                        o.Send("The sun disappears behind a blanket of clouds.");
                        break;
                    case Conditions.Drizzling:
                        o.Send("A light rain begins to fall from the sky.");
                        break;
                    case Conditions.Raining:
                        o.Send("It starts raining heavily.");
                        break;
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateWind(IObject o)
        {
            if (this.CurrentWindSpeed == this.NextWindSpeed)
            {
                List<WindSpeeds> list = new List<WindSpeeds>();
                for (int x = 0; x < this.PossibleWindSpeeds.Count; ++x)
                    if (this.PossibleWindSpeeds.Get(x))
                        list.Add((WindSpeeds)x);

                if (list.Count == 0)
                    return;
                else if (list.Count == 1 || !list.Contains(this.CurrentWindSpeed))
                {
                    this.CurrentWindSpeed = list[0];
                    return;
                }

                this.NextWindSpeed = list[Globals.Random.Next(list.Count)];
            }

            if (this.NextWindSpeed < this.CurrentWindSpeed)
                --this.CurrentWindSpeed;
            else if (this.NextWindSpeed > this.CurrentWindSpeed)
                ++this.CurrentWindSpeed;
        }
        
        [Obfuscation(Exclude = false)]
        public object Clone()
        {
            WeatherConditions other = this.MemberwiseClone() as WeatherConditions;
            other.possibleconditions_ = new BitArray(this.possibleconditions_);
            other.possibletemps_ = new BitArray(this.possibletemps_);
            other.possiblewinds_ = new BitArray(this.possiblewinds_);
            return other;
        }
    }
}
