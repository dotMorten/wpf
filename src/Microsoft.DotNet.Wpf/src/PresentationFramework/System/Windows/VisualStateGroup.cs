// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows
{
    /// <summary>
    ///     A group of mutually exclusive visual states.
    /// </summary>
    [ContentProperty("States")] 
    [RuntimeNameProperty("Name")]
    public class VisualStateGroup : DependencyObject 
    {
        /// <summary>
        ///     The Name of the VisualStateGroup.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        ///     VisualStates in the group.
        /// </summary>
        public IList States
        {
            get
            {
                if (_states == null)
                {
                    _states = new FreezableCollection<VisualState>();
                }

                return _states;
            }
        }

        /// <summary>
        ///     Transitions between VisualStates in the group.
        /// </summary>
        public IList Transitions
        {
            get
            {
                if (_transitions == null)
                {
                    _transitions = new FreezableCollection<VisualTransition>();
                }

                return _transitions;
            }
        }

        /// <summary>
        ///     VisualState that is currently applied.
        /// </summary>
        public VisualState CurrentState 
        { 
            get; 
            internal set; 
        }

        internal VisualState GetState(string stateName)
        {
            for (int stateIndex = 0; stateIndex < States.Count; ++stateIndex)
            {
                VisualState state = (VisualState)States[stateIndex];
                if (state.Name == stateName)
                {
                    return state;
                }
            }

            return null;
        }

        internal Collection<Storyboard> CurrentStoryboards
        {
            get
            {
                if (_currentStoryboards == null)
                {
                    _currentStoryboards = new Collection<Storyboard>();
                }

                return _currentStoryboards;
            }
        }

        // This will transfer information in the _setters collection to PropertyValues array.
        internal void ProcessSettersCollection(SetterBaseCollection setters)
        {
            // Add information in Setters collection to PropertyValues array.
            if (setters != null)
            {
                // Seal Setters
                setters.Seal();

                for (int i = 0; i < setters.Count; i++)
                {
                    Setter setter = setters[i] as Setter;
                    if (setter != null)
                    {
                        DependencyProperty dp = setter.Property;
                        object value = setter.ValueInternal;
                        string target = setter.TargetName;

                        if (target == null)
                        {
                            ProcessParametersContainer(dp);

                            target = StyleHelper.SelfName;
                        }
                        else
                        {
                            target = ProcessParametersVisualTreeChild(dp, target); // name string will get interned
                        }

                        DynamicResourceExtension dynamicResource = value as DynamicResourceExtension;
                        if (dynamicResource == null)
                        {
                            AddToPropertyValues(target, dp, value, PropertyValueType.Trigger);
                        }
                        else
                        {
                            AddToPropertyValues(target, dp, dynamicResource.ResourceKey, PropertyValueType.PropertyTriggerResource);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(SR.Get(SRID.VisualTriggerSettersIncludeUnsupportedSetterType, setters[i].GetType().Name));
                    }
                }
            }
        }

        /// <summary>
        ///     Parameter validation work common to the SetXXXX methods that deal
        /// with visual tree child nodes.
        /// </summary>
        internal string ProcessParametersVisualTreeChild(DependencyProperty dp, string target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (target.Length == 0)
            {
                throw new ArgumentException(SR.Get(SRID.ChildNameMustBeNonEmpty));
            }

            return String.Intern(target);
        }

        /// <summary>
        ///     After the parameters have been validated, store it in the
        /// PropertyValues collection.
        /// </summary>
        /// <remarks>
        ///     All these will be looked at again (and processed into runtime
        /// data structures) by Style.Seal(). We keep them around even after
        /// that point should we need to serialize this data back out.
        /// </remarks>
        internal void AddToPropertyValues(string childName, DependencyProperty dp, object value, PropertyValueType valueType)
        {
            // Store original data
            PropertyValue propertyValue = new PropertyValue();
            propertyValue.ValueType = valueType;
            propertyValue.Conditions = null;  // Delayed - derived class is responsible for this item.
            propertyValue.ChildName = childName;
            propertyValue.Property = dp;
            propertyValue.ValueInternal = value;

            PropertyValues.Add(propertyValue);
        }
        /* property */
        internal MS.Utility.FrugalStructList<System.Windows.PropertyValue> PropertyValues = new MS.Utility.FrugalStructList<System.Windows.PropertyValue>();

        /// <summary>
        ///     Parameter validation work common to the SetXXXX methods that deal
        /// with the container node of the Style/Template.
        /// </summary>
        internal void ProcessParametersContainer(DependencyProperty dp)
        {
            // Not allowed to use Style to affect the StyleProperty.
            if (dp == FrameworkElement.StyleProperty)
            {
                throw new ArgumentException(SR.Get(SRID.StylePropertyInStyleNotAllowed));
            }
        }
        internal void StartNewThenStopOld(FrameworkElement element, params Storyboard[] newStoryboards)
        {
            // Remove the old Storyboards. Remove is delayed until the next TimeManager tick, so the
            // handoff to the new storyboard is unaffected.
            for (int index = 0; index < CurrentStoryboards.Count; ++index)
            {
                if (CurrentStoryboards[index] == null)
                {
                    continue;
                }

                CurrentStoryboards[index].Remove(element);
            }
            CurrentStoryboards.Clear();

            // Start the new Storyboards
            for (int index = 0; index < newStoryboards.Length; ++index)
            {
                if (newStoryboards[index] == null)
                {
                    continue;
                }

                newStoryboards[index].Begin(element, HandoffBehavior.SnapshotAndReplace, true);
                
                // Hold on to the running Storyboards
                CurrentStoryboards.Add(newStoryboards[index]);

                // Silverlight had an issue where initially, a checked CheckBox would not show the check mark
                // until the second frame. They chose to do a Seek(0) at this point, which this line
                // is supposed to mimic. It does not seem to be equivalent, though, and WPF ends up
                // with some odd animation behavior. I haven't seen the CheckBox issue on WPF, so
                // commenting this out for now.
                // newStoryboards[index].SeekAlignedToLastTick(element, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            }

        }

        internal void RaiseCurrentStateChanging(FrameworkElement stateGroupsRoot, VisualState oldState, VisualState newState, FrameworkElement control)
        {
            if (CurrentStateChanging != null)
            {
                CurrentStateChanging(stateGroupsRoot, new VisualStateChangedEventArgs(oldState, newState, control, stateGroupsRoot));
            }
        }

        internal void RaiseCurrentStateChanged(FrameworkElement stateGroupsRoot, VisualState oldState, VisualState newState, FrameworkElement control)
        {
            if (CurrentStateChanged != null)
            {
                CurrentStateChanged(stateGroupsRoot, new VisualStateChangedEventArgs(oldState, newState, control, stateGroupsRoot));
            }
        }

        /// <summary>
        ///     Raised when transition begins
        /// </summary>
        public event EventHandler<VisualStateChangedEventArgs> CurrentStateChanged;

        /// <summary>
        ///     Raised when transition ends and new state storyboard begins.
        /// </summary>
        public event EventHandler<VisualStateChangedEventArgs> CurrentStateChanging;

        private Collection<Storyboard> _currentStoryboards;
        private FreezableCollection<VisualState> _states;
        private FreezableCollection<VisualTransition> _transitions;
    } 
}
