// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

﻿
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows
{
    /// <summary>
    ///     A visual state that can be transitioned into.
    /// </summary>
    [ContentProperty("Storyboard")]
    [RuntimeNameProperty("Name")]
    public class VisualState : DependencyObject, System.Windows.Markup.IAddChild
    {
        /// <summary>
        ///     The name of the VisualState.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
		
        /// <summary>
        ///     The name of the VisualState.
        /// </summary>
        public SetterBaseCollection Setters
        {
            get;
        } = new SetterBaseCollection();
		
		public System.Collections.Generic.IList<StateTriggerBase> StateTriggers 
		{
			get; 
		} = new StateTriggerBaseCollection();

        private static readonly DependencyProperty StoryboardProperty = 
            DependencyProperty.Register(
            "Storyboard", 
            typeof(Storyboard), 
            typeof(VisualState));

        /// <summary>
        ///     Storyboard defining the values of properties in this visual state.
        /// </summary>        
        public Storyboard Storyboard
        {
            get { return (Storyboard)GetValue(StoryboardProperty); }
            set { SetValue(StoryboardProperty, value); }
        }

        void System.Windows.Markup.IAddChild.AddChild(object child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            if (child is StateTriggerBase trigger)
            {
                StateTriggers.Add(trigger);
            }
            else
            {
                throw new ArgumentException(SR.Get(SRID.Animation_ChildMustBeKeyFrame), "child"); // TODO: Fix string
            }
        }
        void System.Windows.Markup.IAddChild.AddText(string text)
        {
            throw new InvalidOperationException(SR.Get(SRID.Animation_NoTextChildren)); // TODO: Fix string
        }
    }
}
