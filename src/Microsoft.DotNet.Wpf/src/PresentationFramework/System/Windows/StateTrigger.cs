// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/***************************************************************************\
*
*
* A collection of SetterBase-derived classes. See use in Style.cs and other
* places.
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
    ///     Represents a trigger that applies visual states conditionally.
    /// </summary>
    public sealed class StateTrigger : StateTriggerBase
    {
		/// <summary>
		///      Initializes a new instance of the <see cref="StateTrigger" /> class.
		/// </summary>
		public StateTrigger() : base()
		{
		}		
			
        /// <summary>
        ///     Identifies the <see cref="IsActive" /> dependency property.
        /// </summary>        
        public static readonly DependencyProperty IsActiveProperty = 
            DependencyProperty.Register(
            "IsActive", 
            typeof(bool), 
            typeof(StateTrigger), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsActiveChanged)));

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StateTrigger) d).SetActive((bool) e.NewValue);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the trigger should be applied.
        /// </summary>        
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
    }
}
