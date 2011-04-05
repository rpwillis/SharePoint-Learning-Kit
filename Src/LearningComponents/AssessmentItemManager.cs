/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.LearningComponents.DataModel;

namespace Microsoft.LearningComponents
{
    /// <summary>
    /// Creates dictionaries of FormDataProcessor's and AssessmentItemRenderer's, so that only single instances
    /// of them exist to process form data and render assessment items.
    /// </summary>
    internal class AssessmentItemManager
    {
        // Collection of objects that can process form data. 
        private Dictionary<InteractionType, FormDataProcessor> m_formProcessors;

        // Collection of objects that can render LRM content
        private Dictionary<AssessmentItemType, AssessmentItemRenderer> m_renderers;

        private RloDataModelContext m_dataModelContext;
        private RloProcessFormDataContext m_pfContext; 

        /// <summary>
        /// Used when rendering. 
        /// </summary>
        internal AssessmentItemManager()
        {
        }

        /// <summary>
        /// Gets the RenderContext, or <c>null</c> if the current context is not a <Typ>RloRenderContext</Typ>.
        /// </summary>
        internal RloRenderContext RenderContext
        {
            get
            {
                return m_dataModelContext as RloRenderContext;
            }
        }

        /// <summary>
        /// Sets or gets m_dataModelContext.  When setting m_dataModelContext for the first time, creates
        /// m_renderers.
        /// </summary>
        internal RloDataModelContext DataModelContext
        {
            get
            {
                if (m_dataModelContext == null)
                    throw new LearningComponentsInternalException("AIM01");
                return m_dataModelContext;
            }
            set
            {
                // Disallow setting this to null.
                if (value == null) throw new LearningComponentsInternalException("AIM03");
                // If setting to a new value, set it to the new value and create or update the assessment renderers.
                if (m_dataModelContext == null || !m_dataModelContext.Equals(value))
                {
                    // If this is the first time RenderContext has been set, create the renderers.
                    if (m_renderers == null)
                    {
                        m_renderers = new Dictionary<AssessmentItemType, AssessmentItemRenderer>(10);
                        m_renderers.Add(AssessmentItemType.Checkbox, new CheckboxAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.File, new FileAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.ItemScore, new ItemScoreAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.Radio, new RadioAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.Rubric, new RubricAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.Select, new SelectAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.Text, new TextAssessmentRenderer(value));
                        m_renderers.Add(AssessmentItemType.TextArea, new TextAreaAssessmentRenderer(value));
                    }
                    // If the renderers already exist, update their render contexts.
                    else
                    {
                        foreach (AssessmentItemRenderer ai in m_renderers.Values)
                        {
                            ai.DataModelContext = value;
                        }
                    }
                    m_dataModelContext = value;
                }
            }
        }

        /// <summary>
        /// Sets or gets m_pfContext.  When setting m_pfContext for the first time, sets m_formProcessors.
        /// </summary>
        internal RloProcessFormDataContext ProcessFormContext
        {
            get
            {
                if (m_pfContext == null)
                    throw new LearningComponentsInternalException("AIM02");
                return m_pfContext;
            }
            set
            {
                // Disallow setting this to null.
                if (value == null) throw new LearningComponentsInternalException("AIM03");
                // If setting to a new value, set it to the new value and create or update the form data processors.
                if (m_pfContext != value)
                {
                    if (m_formProcessors == null)
                    {
                        m_formProcessors = new Dictionary<InteractionType, FormDataProcessor>(10);
                        m_formProcessors.Add(InteractionType.Attachment, new FileAttachmentFormDataProcessor(value));
                        m_formProcessors.Add(InteractionType.Essay, new EssayFormDataProcessor(value));
                        m_formProcessors.Add(InteractionType.FillIn, new FillInFormDataProcessor(value));
                        m_formProcessors.Add(InteractionType.MultipleChoice, new MultipleChoiceFormDataProcessor(value));
                        m_formProcessors.Add(InteractionType.Matching, new MatchingFormDataProcessor(value));
                    }
                    // If the form data processors already exist, update their contexts.
                    else
                    {
                        foreach (FormDataProcessor p in m_formProcessors.Values)
                        {
                            p.Context = value;
                        }
                    }
                    m_pfContext = value;
                }
            }
        }

        /// <summary>
        /// Gets the renderer that can render an assessmentItem.
        /// </summary>
        /// <param name="assessmentItem">The assessment item to render. This is the &lt;img&gt; element that is in 
        /// the original LRM content.</param>
        /// <remarks>RenderContext must have been set prior to calling this method.</remarks>
        internal AssessmentItemRenderer GetRenderer(AssessmentItem assessmentItem)
        {
            AssessmentItemRenderer renderer = m_renderers[assessmentItem.Type];
            renderer.AssessmentItem = assessmentItem;
            return renderer;
        }

        /// <summary>
        /// Gets the processor that can process posted data related to the specified interaction, or <c>null</c> if none.
        /// </summary>
        /// <remarks>ProcessFormContext must have been set prior to calling this method.  If this
        /// interaction's InteractionType does not have a value (such as when it represents a rubric or item score
        /// with no associated assessment) this method returns <c>null</c>.
        /// </remarks>
        internal FormDataProcessor GetFormDataProcessor(Interaction interaction)
        {
            if (interaction.InteractionType.HasValue)
            {
                FormDataProcessor processor = m_formProcessors[interaction.InteractionType.Value];
                processor.Interaction = interaction;
                return processor;
            }
            else
            {
                return null;
            }
        }
    }
}
