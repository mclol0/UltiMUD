using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Damdai
{
    public delegate void Function_void();
    public delegate void Function_void<T>(T t);
    public delegate void Function_void<T1, T2>(T1 t1, T2 t2);
    public delegate void Function_void<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate void Function_void<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void Function_void<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public delegate void Function_void<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate void Function_void<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate void Function_void<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
    public delegate void Function_void<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);
    public delegate R Function<R>();
    public delegate R Function<R, T>(T t);
    public delegate R Function<R, T1, T2>(T1 t1, T2 t2);
    public delegate R Function<R, T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate R Function<R, T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate R Function<R, T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public delegate R Function<R, T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
    public delegate R Function<R, T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
    public delegate R Function<R, T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
    public delegate R Function<R, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9);

    public delegate void Function_void_r<T1, T2>(T1 t1, ref T2 t2);
    public delegate R Function_r<R, T1, T2>(T1 t1, ref T2 t2);
    public delegate void Function_voidr<R, T1, T2, T3>(ref T1 t1, T2 t2, T3 t3);
    public delegate R Functionr<R, T1, T2, T3>(ref T1 t1, T2 t2, T3 t3);

    public static class Async
    {
        public struct Error
        {
            private bool failed;
            public Exception Exception;
            public List<object> Parameters;

            public Error(Exception e, params object[] parameters)
            {
                this.failed = true;
                this.Exception = e;
                this.Parameters = new List<object>(parameters);
            }
            public static implicit operator bool(Error value)
            {
                return value.failed;
            }
        }

        public static Function_void Function<R>(Function<R> fn, Function_void<Error, R> continuation)
        {
            return delegate()
            {
                fn.BeginInvoke(delegate(IAsyncResult ar)
                {
                    try
                    {
                        R result = ((Function<R>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error(), result);
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                    catch (Exception e)
                    {
                        continuation(new Error(e), default(R));
                    }
                }, fn);
            };
        }
        public static Function_void<A1> Function<R, A1>(Function<R, A1> fn, Function_void<Error, R> continuation)
        {
            return delegate(A1 a1)
            {
                fn.BeginInvoke(a1, delegate(IAsyncResult ar)
                {
                    try
                    {
                        R result = ((Function<R, A1>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error(), result);
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                    catch (Exception e)
                    {
                        continuation(new Error(e, a1), default(R));
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2> Function<R, A1, A2>(Function<R, A1, A2> fn, Function_void<Error, R> continuation)
        {
            return delegate(A1 a1, A2 a2)
            {
                fn.BeginInvoke(a1, a2, delegate(IAsyncResult ar)
                {
                    try
                    {
                        R result = ((Function<R, A1, A2>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error(), result);
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                    catch (Exception e)
                    {
                        continuation(new Error(e, a1, a2), default(R));
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2, A3> Function<R, A1, A2, A3>(Function<R, A1, A2, A3> fn, Function_void<Error, R> continuation)
        {
            return delegate(A1 a1, A2 a2, A3 a3)
            {
                fn.BeginInvoke(a1, a2, a3, delegate(IAsyncResult ar)
                {
                    try
                    {
                        R result = ((Function<R, A1, A2, A3>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error(), result);
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                    catch (Exception e)
                    {
                        continuation(new Error(e, a1, a2, a3), default(R));
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2, A3, A4> Function<R, A1, A2, A3, A4>(Function<R, A1, A2, A3, A4> fn, Function_void<Error, R> continuation)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4)
            {
                fn.BeginInvoke(a1, a2, a3, a4, delegate(IAsyncResult ar)
                {
                    try
                    {
                        R result = ((Function<R, A1, A2, A3, A4>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error(), result);
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                    catch (Exception e)
                    {
                        continuation(new Error(e, a1, a2, a3, a4), default(R));
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2, A3, A4, A5> Function<R, A1, A2, A3, A4, A5>(Function<R, A1, A2, A3, A4, A5> fn, Function_void<Error, R> continuation)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
            {
                fn.BeginInvoke(a1, a2, a3, a4, a5, delegate(IAsyncResult ar)
                {
                    try
                    {
                        R result = ((Function<R, A1, A2, A3, A4, A5>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error(), result);
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                    catch (Exception e)
                    {
                        continuation(new Error(e, a1, a2, a3, a4, a5), default(R));
                    }
                }, fn);
            };
        }

        public static Function_void Function_void(Function_void fn, Function_void<Error> continuation)
        {
            return delegate()
            {
                fn.BeginInvoke(delegate(IAsyncResult ar)
                {
                    try
                    {
                        ((Function_void)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error());
                    }
                    catch (Exception e)
                    {
                        if (continuation != null)
                            continuation(new Error(e));
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                }, fn);
            };
        }
        public static Function_void<A1> Function_void<A1>(Function_void<A1> fn, Function_void<Error> continuation)
        {
            return delegate(A1 a1)
            {
                fn.BeginInvoke(a1, delegate(IAsyncResult ar)
                {
                    try
                    {
                        ((Function_void<A1>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error());
                    }
                    catch (Exception e)
                    {
                        if (continuation != null)
                            continuation(new Error(e, a1));
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2> Function_void<A1, A2>(Function_void<A1, A2> fn, Function_void<Error> continuation)
        {
            return delegate(A1 a1, A2 a2)
            {
                fn.BeginInvoke(a1, a2, delegate(IAsyncResult ar)
                {
                    try
                    {
                        ((Function_void<A1, A2>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error());
                    }
                    catch (Exception e)
                    {
                        if (continuation != null)
                            continuation(new Error(e, a1, a2));
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2, A3> Function_void<A1, A2, A3>(Function_void<A1, A2, A3> fn, Function_void<Error> continuation)
        {
            return delegate(A1 a1, A2 a2, A3 a3)
            {
                fn.BeginInvoke(a1, a2, a3, delegate(IAsyncResult ar)
                {
                    try
                    {
                        ((Function_void<A1, A2, A3>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error());
                    }
                    catch (Exception e)
                    {
                        if (continuation != null)
                            continuation(new Error(e, a1, a2, a3));
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2, A3, A4> Function_void<A1, A2, A3, A4>(Function_void<A1, A2, A3, A4> fn, Function_void<Error> continuation)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4)
            {
                fn.BeginInvoke(a1, a2, a3, a4, delegate(IAsyncResult ar)
                {
                    try
                    {
                        ((Function_void<A1, A2, A3, A4>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error());
                    }
                    catch (Exception e)
                    {
                        if (continuation != null)
                            continuation(new Error(e, a1, a2, a3, a4));
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                }, fn);
            };
        }
        public static Function_void<A1, A2, A3, A4, A5> Function_void<A1, A2, A3, A4, A5>(Function_void<A1, A2, A3, A4, A5> fn, Function_void<Error> continuation)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
            {
                fn.BeginInvoke(a1, a2, a3, a4, a5, delegate(IAsyncResult ar)
                {
                    try
                    {
                        ((Function_void<A1, A2, A3, A4, A5>)ar.AsyncState).EndInvoke(ar);
                        if (!ar.IsCompleted)
                            throw new ApplicationException("Async call incomplete.");
                        else if (continuation != null)
                            continuation(new Error());
                    }
                    catch (Exception e)
                    {
                        if (continuation != null)
                            continuation(new Error(e, a1, a2, a3, a4, a5));
                        else
                            throw new ApplicationException("Unknown Async failure.");
                    }
                }, fn);
            };
        }
    }

    public static class Ctrl
    {
        public static Function_void Function_void(Control control, Function_void fn, bool flush)
        {
            return delegate()
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(fn);

                    if (flush)
                        Application.DoEvents();
                }
                else
                    fn();
            };
        }
        public static Function_void Function_void(Control control, Function_void fn)
        {
            return delegate()
            {
                if (control.InvokeRequired)
                    control.Invoke(fn);
                else
                    fn();
            };
        }
        public static Function_void<A1> Function_void<A1>(Control control, Function_void<A1> fn, bool flush)
        {
            return delegate(A1 a1)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(fn, a1);

                    if (flush)
                        Application.DoEvents();
                }
                else
                    fn(a1);
            };
        }
        public static Function_void<A1> Function_void<A1>(Control control, Function_void<A1> fn)
        {
            return delegate(A1 a1)
            {
                if (control.InvokeRequired)
                    control.Invoke(fn, a1);
                else
                    fn(a1);
            };
        }
        public static Function_void<A1, A2> Function_void<A1, A2>(Control control, Function_void<A1, A2> fn, bool flush)
        {
            return delegate(A1 a1, A2 a2)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(fn, a1, a2);

                    if (flush)
                        Application.DoEvents();
                }
                else
                    fn(a1, a2);
            };
        }
        public static Function_void<A1, A2> Function_void<A1, A2>(Control control, Function_void<A1, A2> fn)
        {
            return delegate(A1 a1, A2 a2)
            {
                if (control.InvokeRequired)
                    control.Invoke(fn, a1, a2);
                else
                    fn(a1, a2);
            };
        }
        public static Function_void<A1, A2, A3> Function_void<A1, A2, A3>(Control control, Function_void<A1, A2, A3> fn, bool flush)
        {
            return delegate(A1 a1, A2 a2, A3 a3)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(fn, a1, a2, a3);

                    if (flush)
                        Application.DoEvents();
                }
                else
                    fn(a1, a2, a3);
            };
        }
        public static Function_void<A1, A2, A3> Function_void<A1, A2, A3>(Control control, Function_void<A1, A2, A3> fn)
        {
            return delegate(A1 a1, A2 a2, A3 a3)
            {
                if (control.InvokeRequired)
                    control.Invoke(fn, a1, a2, a3);
                else
                    fn(a1, a2, a3);
            };
        }
        public static Function_void<A1, A2, A3, A4> Function_void<A1, A2, A3, A4>(Control control, Function_void<A1, A2, A3, A4> fn, bool flush)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(fn, a1, a2, a3, a4);

                    if (flush)
                        Application.DoEvents();
                }
                else
                    fn(a1, a2, a3, a4);

            };
        }
        public static Function_void<A1, A2, A3, A4> Function_void<A1, A2, A3, A4>(Control control, Function_void<A1, A2, A3, A4> fn)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4)
            {
                if (control.InvokeRequired)
                    control.Invoke(fn, a1, a2, a3, a4);
                else
                    fn(a1, a2, a3, a4);
            };
        }
        public static Function_void<A1, A2, A3, A4, A5> Function_void<A1, A2, A3, A4, A5>(Control control, Function_void<A1, A2, A3, A4, A5> fn, bool flush)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(fn, a1, a2, a3, a4, a5);

                    if (flush)
                        Application.DoEvents();
                }
                else
                    fn(a1, a2, a3, a4, a5);
            };
        }
        public static Function_void<A1, A2, A3, A4, A5> Function_void<A1, A2, A3, A4, A5>(Control control, Function_void<A1, A2, A3, A4, A5> fn)
        {
            return delegate(A1 a1, A2 a2, A3 a3, A4 a4, A5 a5)
            {
                if (control.InvokeRequired)
                    control.Invoke(fn, a1, a2, a3, a4, a5);
                else
                    fn(a1, a2, a3, a4, a5);
            };
        }
    }
}
