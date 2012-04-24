/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Xml;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

//
// Disposer.cs
//
// This file defines a Disposer class that helps handle disposable objects
//

namespace Microsoft.LearningComponents
{

    // Disposer
    //
    // This utility class implements a stack of disposable objects, and is
    // intended for use within a "using" statement, e.g.:
    //
    //     using (Disposer disposer = new Disposer())
    //     {
    //         ...
    //         Font font = new Font("Arial", 10.0f);
    //         disposer.Push(font);
    //         ...
    //         FileStream fs = new FileStream(...);
    //         disposer.Push(fs);
    //         ...
    //     } // at this point, IDisposable.Dispose is called on <fs>, then <font>
    //
    // This implements scope-level object cleanup (as in C++):
    //
    // When Disposer's IDisposable.Dispose method is called (either explicitly,
    // or implicitly by a "using" statement), the object references that were
    // pushed onto the Disposer stack are disposed of in the reverse order in
    // which they were pushed on.
    //
    internal class Disposer : IDisposable
    {
        private Stack m_stack = new Stack(); // stack of objects to dispose of
        public void Push(IDisposable d)
        {
            // implement Push() instead of inheriting from Stack to check that
            // <d> implements IDisposable at the time Push() is called
            if (d != null)
                m_stack.Push(d);
        }
        public void Dispose()
        {
            while (m_stack.Count > 0)
            {
                IDisposable d = (IDisposable) m_stack.Pop();
                d.Dispose();
            }
        }
    }
}
