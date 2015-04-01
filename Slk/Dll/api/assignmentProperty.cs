using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>The type of property.</summary>
    public enum AssignmentPropertyType
    {
        /// <summary>A string property.</summary>
        Text,
        /// <summary>A choice property.</summary>
        Choice,
        /// <summary>A url property.</summary>
        Url
    }

    /// <summary>A custom property for an assignment.</summary>
    public abstract class AssignmentProperty
    {
#region constructors
        /// <summary>Initializes a new instance of <see cref="AssignmentProperty"/>.</summary>
        public AssignmentProperty(string title, string name, AssignmentPropertyType type, bool required)
        {
            Name = name;
            Title = title;
            Required = required;
            Type = type;
        }
#endregion constructors

#region properties
        /// <summary>The name of the property.</summary>
        public string Name { get; private set; }
        /// <summary>The title of the property.</summary>
        public string Title { get; private set; }
        /// <summary>The value of the property.</summary>
        public virtual string Value { get; set; }
        /// <summary>The type of the property.</summary>
        public AssignmentPropertyType Type { get; private set; }
        /// <summary>Whether the property is required.</summary>
        public bool Required { get; private set; }
#endregion properties

#region public methods
        /// <summary>Renders the property input for editing.</summary>
        public virtual void RenderForEdit()
        {
        }

        /// <summary>Renders the property for viewing.</summary>
        public virtual void RenderForView()
        {
            throw new NotSupportedException();
        }
#endregion public methods

#region protected methods
#endregion protected methods

#region private methods
#endregion private methods

#region static members
#endregion static members
    }

#region TextAssignmentProperty
    /// <summary>An assignment property which is a string.</summary>
    public class TextAssignmentProperty : AssignmentProperty
    {
        /// <summary>Initializes a new instance of <see cref="TextAssignmentProperty"/>.</summary>
        public TextAssignmentProperty(string title, string name, bool required) : base (title, name, AssignmentPropertyType.Text, required)
        {
        }

        /// <summary>Shows if a multi-line value.</summary>
        public bool IsMultiLine { get; set; }
    }
#endregion TextAssignmentProperty

#region UrlAssignmentProperty
    /// <summary>An assignment property which is a url.</summary>
    public class UrlAssignmentProperty : AssignmentProperty
    {
        /// <summary>Initializes a new instance of <see cref="UrlAssignmentProperty"/>.</summary>
        public UrlAssignmentProperty(string title, string name, bool required) : base (title, name, AssignmentPropertyType.Url, required)
        {
        }

        /// <summary>See <see cref="AssignmentProperty.Value "/>.</summary>
        public override string Value
        { 
            get { return base.Value ;}
            set
            {
                // url must be server relative or be absolute
                string url = value;

                if (string.IsNullOrEmpty(url) == false)
                {
                    if (url[0] != '/' && url.Contains("://") == false)
                    {
                        url = "http://" + value;
                    }
                }

                base.Value = url;
            }
        }
    }
#endregion UrlAssignmentProperty

#region ChoiceAssignmentProperty
    /// <summary>An assignment property which is a choice field.</summary>
    public class ChoiceAssignmentProperty : AssignmentProperty
    {
        /// <summary>Initializes a new instance of <see cref="ChoiceAssignmentProperty"/>.</summary>
        public ChoiceAssignmentProperty(string title, string name, bool required, StringCollection choices, string defaultValue) : base (title, name, AssignmentPropertyType.Choice, required)
        {
            DefaultValue = defaultValue;
            Value = DefaultValue;
            Choices = new List<string>();
            foreach (string choice in choices)
            {
                Choices.Add(choice);
            }
        }

        /// <summary>The default value.</summary>
        public string DefaultValue { get; private set; }

        /// <summary>The choices.</summary>
        public ICollection<string> Choices { get; private set; }
    }
#endregion ChoiceAssignmentProperty
}

