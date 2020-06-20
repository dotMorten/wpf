// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/***************************************************************************\
*
*
* Represents the base class for state triggers.
*
*
\***************************************************************************/
using System.Collections.ObjectModel; // Collection<T>
using System.Diagnostics;   // Debug.Assert
using System.Windows.Data;  // Binding knowledge
using System.Windows.Media; // Visual knowledge
using System.Windows.Markup; // MarkupExtension

namespace System.Windows
{
    /// <summary>
    ///     Represents the base class for state triggers.
    /// </summary>
    public class StateTriggerBase : DependencyObject
    {
        private bool _isActive;

        /// <summary>
		///      Initializes a new instance of the <see cref="StateTriggerBase" /> class.
		/// </summary>
		protected StateTriggerBase()
        {
        }

        /// <summary>
		///     Sets the value that indicates whether the state trigger is active.
	    /// </summary>
        protected void SetActive(bool IsActive)
        {
            if (_isActive != IsActive) 
            {
                _isActive = IsActive;
                // TODO
                // VisualStateManager.GetCustomVisualStateManager
            }
        }
    }
}
