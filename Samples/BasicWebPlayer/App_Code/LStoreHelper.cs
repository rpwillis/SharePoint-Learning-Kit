/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MICROSOFT PROVIDES SAMPLE CODE "AS IS" AND WITH ALL FAULTS, AND WITHOUT ANY WARRANTY WHATSOEVER.  
// MICROSOFT EXPRESSLY DISCLAIMS ALL WARRANTIES WITH RESPECT TO THE SOURCE CODE, INCLUDING BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  THERE IS 
// NO WARRANTY OF TITLE OR NONINFRINGEMENT FOR THE SOURCE CODE.
// LStoreHelper.cs
//
// Implements LStoreHelper, a class containing static helper methods to assist with
// LearningStore-related operations.
//

using System;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;

namespace LearningComponentsHelper
{

// <summary>
// Static helper methods to assist with LearningStore-related operations.
// </summary>
//
// <remarks>
// This class is defined using the "partial" keyword so that additional application-specific
// helper methods (leveraging, for example, application-specific LearningStore schema extensions)
// can be added.  (Inheritance is not used here because static classes cannot derive from other
// static classes.)  For example, see SlkLStoreHelper.cs.
// </remarks>
//
public static partial class LStoreHelper
{
	/// <summary>
	/// Converts a value returned from a LearningStore query to a given type, or <c>null</c>
	/// if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.  <paramref name="value"/> is cast to
	/// 	the type of <paramref name="result"/>, or is set to <c>null</c> if
	/// 	<paramref name="value"/> is <c>DBNull</c>.</param>
	///
	/// <remarks>
	/// This version of <c>Cast</c> is used to retrieve a <i>value type</i> from LearningStore,
	/// i.e. <c>bool</c>, <c>DateTime</c>, <c>float</c>, <c>double</c>, <c>Guid</c>,
	/// <c>int</c>, or an enumerated type.  The output parameter must be a nullable type;
	/// if the input value is the database "NULL" value, then <c>null</c> is stored in the
	/// output parameter.
	/// </remarks>
	///
	public static void Cast<T>(object value, out T? result) where T : struct
	{
		if (value is DBNull)
			result = null;
		else
			result = (T) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a given type.  Throws an
	/// exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.  <paramref name="value"/> is cast to
	/// 	the type of <paramref name="result"/>, or is set to <c>null</c> if
	/// 	<paramref name="value"/> is <c>DBNull</c>.</param>
	///
	/// <remarks>
	/// This version of <c>Cast</c> is used to retrieve a <i>value type</i> from LearningStore,
	/// i.e. <c>bool</c>, <c>DateTime</c>, <c>float</c>, <c>double</c>, <c>Guid</c>,
	/// <c>int</c>, or an enumerated type, when you know that the input value will not be the
	/// database "NULL" value.  (If it is, an exception is thrown.)
	/// </remarks>
	///
	public static void CastNonNull<T>(object value, out T result) where T : struct
	{
		if (value is DBNull)
			throw new ArgumentException("Unexpected DBNull value");
		else
			result = (T) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a given type, or <c>null</c>
	/// if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.  <paramref name="value"/> is cast to
	/// 	the type of <paramref name="result"/>, or is set to <c>null</c> if
	/// 	<paramref name="value"/> is <c>DBNull</c>.</param>
	///
	/// <remarks>
	/// This version of <c>Cast</c> is used to retrieve a <i>reference type</i> from
	/// LearningStore, i.e. <c>string</c>, <c>LearningStoreXml</c>, <c>IStreamable</c>, or
	/// <c>LearningStoreItemIdentifier</c>.  If the input value is the database "NULL" value,
	/// then <c>null</c> is stored in the output parameter.
	/// </remarks>
	///
	public static void Cast<T>(object value, out T result) where T : class
	{
		if (value is DBNull)
			result = null;
		else
			result = (T) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a string.  Throws an
	/// exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out string result)
	{
		if (value is DBNull)
			throw new ArgumentException("Unexpected DBNull value");
		else
			result = (string) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a <c>LearningStoreXml</c>.
	/// Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out LearningStoreXml result)
	{
		if (value is DBNull)
			throw new ArgumentException("Unexpected DBNull value");
		else
			result = (LearningStoreXml) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to an <c>IStreamable</c>.  Throws
	/// an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out byte[] result)
	{
		if (value is DBNull)
			throw new ArgumentException("Unexpected DBNull value");
		else
			result = (byte[]) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>LearningStoreItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out LearningStoreItemIdentifier result)
	{
		if (value is DBNull)
			throw new ArgumentException("Unexpected DBNull value");
		else
			result = (LearningStoreItemIdentifier) value;
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ActivityAttemptItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out ActivityAttemptItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new ActivityAttemptItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ActivityAttemptItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out ActivityAttemptItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new ActivityAttemptItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ActivityObjectiveItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out ActivityObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new ActivityObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ActivityObjectiveItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out ActivityObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new ActivityObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ActivityPackageItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out ActivityPackageItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new ActivityPackageItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ActivityPackageItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out ActivityPackageItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new ActivityPackageItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>PackageGlobalObjectiveItemIdentifier</c>, or <c>null</c> if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
    public static void Cast(object value, out PackageGlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new PackageGlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>PackageGlobalObjectiveItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value,
		out PackageGlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new PackageGlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a <c>AttemptItemIdentifier</c>,
	/// or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out AttemptItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new AttemptItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>AttemptItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out AttemptItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new AttemptItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>AttemptObjectiveItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out AttemptObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new AttemptObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>AttemptObjectiveItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out AttemptObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new AttemptObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>CommentFromLearnerItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out CommentFromLearnerItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new CommentFromLearnerItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>CommentFromLearnerItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out CommentFromLearnerItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new CommentFromLearnerItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>CommentFromLmsItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out CommentFromLmsItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new CommentFromLmsItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>CommentFromLmsItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out CommentFromLmsItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new CommentFromLmsItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>CorrectResponseItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out CorrectResponseItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new CorrectResponseItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>CorrectResponseItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out CorrectResponseItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new CorrectResponseItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ExtensionDataItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out ExtensionDataItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new ExtensionDataItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ExtensionDataItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out ExtensionDataItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new ExtensionDataItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>GlobalObjectiveItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out GlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new GlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>GlobalObjectiveItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out GlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new GlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>InteractionItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out InteractionItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new InteractionItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>InteractionItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out InteractionItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new InteractionItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>InteractionObjectiveItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out InteractionObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new InteractionObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>InteractionObjectiveItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out InteractionObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new InteractionObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>LearnerGlobalObjectiveItemIdentifier</c>, or <c>null</c> if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out LearnerGlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new LearnerGlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>LearnerGlobalObjectiveItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value,
		out LearnerGlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new LearnerGlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>MapActivityObjectiveToGlobalObjectiveItemIdentifier</c>, or <c>null</c> if the value
	/// is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value,
		out MapActivityObjectiveToGlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new MapActivityObjectiveToGlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>MapActivityObjectiveToGlobalObjectiveItemIdentifier</c>.  Throws an exception if the
	/// value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value,
		out MapActivityObjectiveToGlobalObjectiveItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new MapActivityObjectiveToGlobalObjectiveItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a <c>PackageItemIdentifier</c>,
	/// or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out PackageItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new PackageItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>PackageItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out PackageItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new PackageItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a <c>ResourceItemIdentifier</c>,
	/// or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out ResourceItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new ResourceItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>ResourceItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out ResourceItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new ResourceItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>SequencingLogEntryItemIdentifier</c>, or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out SequencingLogEntryItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new SequencingLogEntryItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>SequencingLogEntryItemIdentifier</c>.  Throws an exception if the value is
	/// <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out SequencingLogEntryItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new SequencingLogEntryItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a <c>UserItemIdentifier</c>,
	/// or <c>null</c> if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void Cast(object value, out UserItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		Cast(value, out id);
		result = (id == null) ? null : new UserItemIdentifier(id);
	}

	/// <summary>
	/// Converts a value returned from a LearningStore query to a
	/// <c>UserItemIdentifier</c>.  Throws an exception if the value is <c>DBNull</c>.
	/// </summary>
	///
	/// <param name="value">A value from a <c>DataRow</c> within a <c>DataTable</c>
	/// 	returned from a LearningStore query.</param>
	///
	/// <param name="result">Where to store the result.</param>
	///
	public static void CastNonNull(object value, out UserItemIdentifier result)
	{
		LearningStoreItemIdentifier id;
		CastNonNull(value, out id);
		result = new UserItemIdentifier(id);
	}
}

}

