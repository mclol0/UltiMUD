using System;
using System.Collections.Generic;
using System.Text;

namespace Damdai
{
    [Serializable]
    public class Pair<T1, T2>
    {
        public void Call(Function_void<T1, T2> fn) { fn(this.First, this.Second); }
        public R Call<R>(Function<R, T1, T2> fn) { return fn(this.First, this.Second); }
        public void Unwrap(ref T1 t1, ref T2 t2) { t1 = this.First; t2 = this.Second; }

        public T1 First;
        public T2 Second;

        public Pair() { }
        public Pair(T1 t1, T2 t2)
        {
            this.First = t1;
            this.Second = t2;
        }
        public static implicit operator Tuple<T1, T2>(Pair<T1, T2> pair)
        {
            return new Tuple<T1, T2>(pair.First, pair.Second);
        }
    }

    [Serializable]
    public struct Tuple<T1>
    {
        public void Call(Function_void<T1> fn) { fn(this.First); }
        public R Call<R>(Function<R, T1> fn) { return fn(this.First); }
        public void Unwrap(ref T1 t1) { t1 = this.First; }

        public T1 First;

        public Tuple(T1 t1)
        {
            this.First = t1;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2>
    {
        public void Call(Function_void<T1, T2> fn) { fn(this.First, this.Second); }
        public R Call<R>(Function<R, T1, T2> fn) { return fn(this.First, this.Second); }
        public void Unwrap(ref T1 t1, ref T2 t2) { t1 = this.First; t2 = this.Second; }

        public T1 First;
        public T2 Second;

        public Tuple(T1 t1, T2 t2)
        {
            this.First = t1;
            this.Second = t2;
        }
        public static implicit operator Pair<T1, T2>(Tuple<T1, T2> tuple)
        {
            return new Pair<T1, T2>(tuple.First, tuple.Second);
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3>
    {
        public void Call(Function_void<T1, T2, T3> fn) { fn(this.First, this.Second, this.Third); }
        public R Call<R>(Function<R, T1, T2, T3> fn) { return fn(this.First, this.Second, this.Third); }
        public void Unwrap(ref T1 t1, ref T2 t2, ref T3 t3)
        {
            t1 = this.First;
            t2 = this.Second;
            t3 = this.Third;
        }

        public T1 First;
        public T2 Second;
        public T3 Third;

        public Tuple(T1 t1, T2 t2, T3 t3)
        {
            this.First = t1;
            this.Second = t2;
            this.Third = t3;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3, T4>
    {
        public void Call(Function_void<T1, T2, T3, T4> fn) { fn(this.First, this.Second, this.Third, this.Fourth); }
        public R Call<R>(Function<R, T1, T2, T3, T4> fn) { return fn(this.First, this.Second, this.Third, this.Fourth); }
        public void Unwrap(ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4)
        {
            t1 = this.First;
            t2 = this.Second;
            t3 = this.Third;
            t4 = this.Fourth;
        }

        public T1 First;
        public T2 Second;
        public T3 Third;
        public T4 Fourth;

        public Tuple(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            this.First = t1;
            this.Second = t2;
            this.Third = t3;
            this.Fourth = t4;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5>
    {
        public void Call(Function_void<T1, T2, T3, T4, T5> fn) { fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth); }
        public R Call<R>(Function<R, T1, T2, T3, T4, T5> fn) { return fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth); }
        public void Unwrap(ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5)
        {
            t1 = this.First;
            t2 = this.Second;
            t3 = this.Third;
            t4 = this.Fourth;
            t5 = this.Fifth;
        }

        public T1 First;
        public T2 Second;
        public T3 Third;
        public T4 Fourth;
        public T5 Fifth;

        public Tuple(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            this.First = t1;
            this.Second = t2;
            this.Third = t3;
            this.Fourth = t4;
            this.Fifth = t5;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5, T6>
    {
        public void Call(Function_void<T1, T2, T3, T4, T5, T6> fn) { fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth, this.Sixth); }
        public R Call<R>(Function<R, T1, T2, T3, T4, T5, T6> fn) { return fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth, this.Sixth); }
        public void Unwrap(ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6)
        {
            t1 = this.First;
            t2 = this.Second;
            t3 = this.Third;
            t4 = this.Fourth;
            t5 = this.Fifth;
            t6 = this.Sixth;
        }

        public T1 First;
        public T2 Second;
        public T3 Third;
        public T4 Fourth;
        public T5 Fifth;
        public T6 Sixth;

        public Tuple(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            this.First = t1;
            this.Second = t2;
            this.Third = t3;
            this.Fourth = t4;
            this.Fifth = t5;
            this.Sixth = t6;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7>
    {
        public void Call(Function_void<T1, T2, T3, T4, T5, T6, T7> fn) { fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth, this.Sixth, this.Seventh); }
        public R Call<R>(Function<R, T1, T2, T3, T4, T5, T6, T7> fn) { return fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth, this.Sixth, this.Seventh); }
        public void Unwrap(ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7)
        {
            t1 = this.First;
            t2 = this.Second;
            t3 = this.Third;
            t4 = this.Fourth;
            t5 = this.Fifth;
            t6 = this.Sixth;
            t7 = this.Seventh;
        }

        public T1 First;
        public T2 Second;
        public T3 Third;
        public T4 Fourth;
        public T5 Fifth;
        public T6 Sixth;
        public T7 Seventh;

        public Tuple(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            this.First = t1;
            this.Second = t2;
            this.Third = t3;
            this.Fourth = t4;
            this.Fifth = t5;
            this.Sixth = t6;
            this.Seventh = t7;
        }
    }

    [Serializable]
    public struct Tuple<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        public void Call(Function_void<T1, T2, T3, T4, T5, T6, T7, T8> fn) { fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth, this.Sixth, this.Seventh, this.Eighth); }
        public R Call<R>(Function<R, T1, T2, T3, T4, T5, T6, T7, T8> fn) { return fn(this.First, this.Second, this.Third, this.Fourth, this.Fifth, this.Sixth, this.Seventh, this.Eighth); }
        public void Unwrap(ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4, ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8)
        {
            t1 = this.First;
            t2 = this.Second;
            t3 = this.Third;
            t4 = this.Fourth;
            t5 = this.Fifth;
            t6 = this.Sixth;
            t7 = this.Seventh;
            t8 = this.Eighth;
        }

        public T1 First;
        public T2 Second;
        public T3 Third;
        public T4 Fourth;
        public T5 Fifth;
        public T6 Sixth;
        public T7 Seventh;
        public T8 Eighth;

        public Tuple(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            this.First = t1;
            this.Second = t2;
            this.Third = t3;
            this.Fourth = t4;
            this.Fifth = t5;
            this.Sixth = t6;
            this.Seventh = t7;
            this.Eighth = t8;
        }
    }
}
