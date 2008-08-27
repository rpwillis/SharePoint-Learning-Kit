#include "stdafx.h"
#include "<!ObjectName!>.H"
#include "ConversionHelper.H"
#include "ServerDataLayer.H"

/****************************************************************************
               Implementacion del objeto T<!ObjectName!>DataKey
****************************************************************************/
T<!ObjectName!>DataKey::T<!ObjectName!>DataKey() 
{
};
	

T<!ObjectName!>DataKey::T<!ObjectName!>DataKey(T<!ObjectName!>DataKey& toCopy, PBaseObject CloneContext) 
{ 
<!ForEachFieldStart(DataKeyType)!>    m_<!FieldName!> = toCopy.m_<!FieldName!>; <!ForEachFieldEnd(DataKeyType)!> 		
};
	
TBaseObject& T<!ObjectName!>DataKey::Clone(PBaseObject CloneContext)
{
	CreateCloneContext(CloneContext, TCloneContextTable);
	TBaseObject& Clone = * new T<!ObjectName!>DataKey(*this, CloneContext);
	DestroyCloneContext(CloneContext);
	return Clone;	
};

bool T<!ObjectName!>DataKey::Equals(TBaseObject& BaseObject)
{
	if (__super::Equals(BaseObject))
	{
		T<!ObjectName!>DataKey& TypedDataKey = static_cast<T<!ObjectName!>DataKey&> (BaseObject);
		bool Equals = true;
		<!ForEachFieldStart(DataKeyType)!>    Equals = Equals && (m_<!FieldName!> == TypedDataKey.m_<!FieldName!>); <!ForEachFieldEnd(DataKeyType)!> 				
		return Equals;		
	
	};
	return false;	
};


#pragma region "Reflection"

void T<!ObjectName!>DataKey::RegisterReflection()
{
	TClassFactory::AddClass("T<!ObjectName!>DataKey", * T<!ObjectName!>DataKey::GetClassRTTI());		
<!ForEachFieldStart(DataKeyType)!>    RegisterProperty<<!FieldType!>>( "<!FieldName!>", &T<!ObjectName!>DataKey::Get<!FieldName!>, &T<!ObjectName!>DataKey::Set<!FieldName!>); <!ForEachFieldEnd(DataKeyType)!> 
};	
	
<!ForEachFieldStart(DataKeyType)!> 
<!FieldType!> T<!ObjectName!>DataKey::Get<!FieldName!>()
{
	return m_<!FieldName!>;	
};

void T<!ObjectName!>DataKey::Set<!FieldName!>(<!FieldType!> Value)
{
	m_<!FieldName!> = Value;
}; <!ForEachFieldEnd(DataKeyType)!> 	

	
#pragma endregion

T<!ObjectName!>DataKey::~T<!ObjectName!>DataKey() 
{
};


/****************************************************************************
               Implementacion del objeto T<!ObjectName!>Criteria
****************************************************************************/
T<!ObjectName!>Criteria::T<!ObjectName!>Criteria() 
{
<!ForEachFieldStart!>    m_Search<!FieldName!> = false; <!ForEachFieldEnd!> 		
};
	

T<!ObjectName!>Criteria::T<!ObjectName!>Criteria(T<!ObjectName!>Criteria& toCopy, PBaseObject CloneContext) 
{ 
<!ForEachFieldStart!>
	m_<!FieldName!> = toCopy.m_<!FieldName!>; 
	m_Search<!FieldName!> = toCopy.m_Search<!FieldName!>; <!ForEachFieldEnd!> 		
};
	
TBaseObject& T<!ObjectName!>Criteria::Clone(PBaseObject CloneContext)
{
	CreateCloneContext(CloneContext, TCloneContextTable);
	TBaseObject& Clone = * new T<!ObjectName!>Criteria(*this, CloneContext);
	DestroyCloneContext(CloneContext);
	return Clone;	
};

bool T<!ObjectName!>Criteria::Equals(TBaseObject& BaseObject)
{
	if (__super::Equals(BaseObject))
	{
		T<!ObjectName!>Criteria& TypedCriteria = static_cast<T<!ObjectName!>Criteria&> (BaseObject);
		bool Equals = true;
		<!ForEachFieldStart!>    Equals = Equals && (m_<!FieldName!> == TypedCriteria.m_<!FieldName!>); <!ForEachFieldEnd!> 				
		return Equals;		
	
	};
	return false;	
};

bool T<!ObjectName!>Criteria::isMatch(T<!ObjectName!>& BusinessObject)
{
	bool Match = true;
<!ForEachFieldStart(ValueType)!>
	if (m_Search<!FieldName!>)
		Match = Match && (BusinessObject.Get<!FieldName!>() == m_<!FieldName!>); <!ForEachFieldEnd(ValueType)!> 				
<!ForEachFieldStart(String)!>
	if (m_Search<!FieldName!>)
		Match = Match && (strcmp(BusinessObject.Get<!FieldName!>(), m_<!FieldName!>) == 0); <!ForEachFieldEnd(String)!> 				

	return Match;
};



#pragma region "Reflection"

void T<!ObjectName!>Criteria::RegisterReflection()
{
	TClassFactory::AddClass("T<!ObjectName!>Criteria", * T<!ObjectName!>Criteria::GetClassRTTI());		
<!ForEachFieldStart!>    RegisterProperty<<!FieldType!>>( "<!FieldName!>", &T<!ObjectName!>Criteria::Get<!FieldName!>, &T<!ObjectName!>Criteria::Set<!FieldName!>); <!ForEachFieldEnd!> 
};	
	
<!ForEachFieldStart!> 
<!FieldType!> T<!ObjectName!>Criteria::Get<!FieldName!>()
{
	return m_<!FieldName!>;	
};

void T<!ObjectName!>Criteria::Set<!FieldName!>(<!FieldType!> Value)
{
	m_<!FieldName!> = Value;
	m_Search<!FieldName!> = true;
}; <!ForEachFieldEnd!> 	

	
#pragma endregion

T<!ObjectName!>Criteria::~T<!ObjectName!>Criteria() 
{
};



/****************************************************************************
               Implementacion del objeto T<!ObjectName!>
****************************************************************************/
TString T<!ObjectName!>::m_TableName;
TString T<!ObjectName!>::m_TableSuffix;

T<!ObjectName!>::T<!ObjectName!>() 
{
<!ForEachFieldStart(String)!>    m_<!FieldName!> = NULL; <!ForEachFieldEnd(String)!> 	
};
	
T<!ObjectName!>::T<!ObjectName!>(T<!ObjectName!>& toCopy, PBaseObject CloneContext) 
{ 
<!ForEachFieldStart(ValueType)!>    m_<!FieldName!> = toCopy.m_<!FieldName!>; <!ForEachFieldEnd(ValueType)!> 
		
<!ForEachFieldStart(String)!>    m_<!FieldName!> = NULL;
    Set<!FieldName!>(toCopy.Get<!FieldName!>()); <!ForEachFieldEnd(String)!> 
};
	
TBaseObject& T<!ObjectName!>::Clone(PBaseObject CloneContext)
{
	CreateCloneContext(CloneContext, TCloneContextTable);
	TBaseObject& Clone = * new T<!ObjectName!>(*this, CloneContext);
	DestroyCloneContext(CloneContext);
	return Clone;	
};

T<!ObjectName!>DataKey& T<!ObjectName!>::GetDataKey()
{	
	<!ForEachFieldStart(DataKeyType)!>    m_DataKey.Set<!FieldName!>(m_<!FieldName!>); <!ForEachFieldEnd(DataKeyType)!> 			
	return m_DataKey;
};

bool T<!ObjectName!>::Equals(TBaseObject& BaseObject)
{
	if (__super::Equals(BaseObject))
	{
		T<!ObjectName!>& TypedObject = static_cast<T<!ObjectName!>&> (BaseObject);
		return GetDataKey().Equals(TypedObject.GetDataKey());	
	};
	return false;	
};

char* T<!ObjectName!>::GetTableName()
{
	if (m_TableName.Data[0] == 0)
	{
		m_TableName.Copy("<!TableName!>");
	};
	return m_TableName.Data;
};


char* T<!ObjectName!>::GetTableSuffix()
{
	if (m_TableSuffix.Data[0] == 0)
	{
		m_TableSuffix.Copy("<!TableSuffix!>");
	};
	return m_TableSuffix.Data;
};


#pragma region "Reflection"

void T<!ObjectName!>::RegisterReflection()
{
	TClassFactory::AddClass("T<!ObjectName!>", * T<!ObjectName!>::GetClassRTTI());		
<!ForEachFieldStart!>    RegisterProperty<<!FieldType!>>( "<!FieldName!>", &T<!ObjectName!>::Get<!FieldName!>, &T<!ObjectName!>::Set<!FieldName!>); <!ForEachFieldEnd!> 
};	
	
<!ForEachFieldStart(ValueType)!> 
<!FieldType!> T<!ObjectName!>::Get<!FieldName!>()
{
	return m_<!FieldName!>;	
};

void T<!ObjectName!>::Set<!FieldName!>(<!FieldType!> Value)
{
	m_<!FieldName!> = Value;
}; <!ForEachFieldEnd(ValueType)!> 	

<!ForEachFieldStart(String)!>
<!FieldType!> T<!ObjectName!>::Get<!FieldName!>()
{
	return m_<!FieldName!>;	
};

void T<!ObjectName!>::Set<!FieldName!>(<!FieldType!> Value)
{
	if (m_<!FieldName!>) delete[] m_<!FieldName!>;
	m_<!FieldName!> = new char[strlen(Value)+1];
	strcpy(m_<!FieldName!>, Value);		
}; <!ForEachFieldEnd(String)!> 	
	
	
#pragma endregion
T<!ObjectName!>* T<!ObjectName!>::GetObject(T<!ObjectName!>DataKey& DataKey)
{
	T<!CollectionName!>& Collection = T<!CollectionName!>::GetCollection();
	for (Collection.GoFirst(); Collection.Actual().classIdConst() != isNullObject; 
		Collection.GoNext())
	{
		T<!ObjectName!>& BusinessObject = static_cast<T<!ObjectName!>&> (Collection.Actual());
		if (BusinessObject.GetDataKey().Equals(DataKey))
			return &BusinessObject;		
	};
	return NULL;
};


T<!ObjectName!>::~T<!ObjectName!>() 
{
<!ForEachFieldStart(String)!>    if (m_<!FieldName!>)	delete[] m_<!FieldName!>; <!ForEachFieldEnd(String)!>
};

/****************************************************************************
               Implementacion del objeto T<!CollectionName!>
****************************************************************************/
T<!CollectionName!>* T<!CollectionName!>::m_CompleteCollection = NULL;

T<!CollectionName!>::T<!CollectionName!>(bool LoadDataFromDB)
{
	if (LoadDataFromDB)
	{
		Managed_Load<!CollectionName!>From(* this);
	};
};
	
#pragma region "Reflection"

void T<!CollectionName!>::RegisterReflection()
{
	TClassFactory::AddClass("T<!CollectionName!>", * T<!CollectionName!>::GetClassRTTI());					
};		
#pragma endregion

T<!CollectionName!>& T<!CollectionName!>::GetCollection()
{
	if (m_CompleteCollection == NULL)
		m_CompleteCollection = new T<!CollectionName!>(true);
	
	return * m_CompleteCollection;
};

T<!CollectionName!>& T<!CollectionName!>::GetCollection(T<!ObjectName!>Criteria& Criteria)
{
	T<!CollectionName!>& Collection = GetCollection();
	T<!CollectionName!>& NewCollection = * new T<!CollectionName!>;
	for (Collection.GoFirst(); Collection.Actual().classIdConst() != isNullObject;
		Collection.GoNext())
	{
		T<!ObjectName!>& BusinessObject = static_cast<T<!ObjectName!>&> (Collection.Actual());
		if (Criteria.isMatch(BusinessObject))
			NewCollection.Add(BusinessObject, false);
	};
	return NewCollection;
};



T<!CollectionName!>::~T<!CollectionName!>()
{

};

#pragma managed(push, on)
void Managed_Load<!CollectionName!>From(T<!CollectionName!>& UnManagedCollection)
{
	BusinessLayerFrameWork::BLCore::BLDataKey^ DataKey;
	BusinessLayerFrameWork::BLCore::IBLListBase^ <!CollectionName!>Collection ;
	System::Type^ EmptyClassType = ServerDataLayer::EmptyClass::typeid;
	System::Reflection::Assembly^ MyAssembly = EmptyClassType ->Assembly;	
	System::Type^ ObjectType = MyAssembly->GetType("ServerDataLayer.<!ObjectName!>");
	System::Type^ CollectionType = MyAssembly->GetType("ServerDataLayer.<!CollectionName!>");

	<!CollectionName!>Collection = static_cast<BusinessLayerFrameWork::BLCore::IBLListBase^> 
		(CollectionType->InvokeMember("GetCollection", 	
		static_cast<System::Reflection::BindingFlags> (
			System::Reflection::BindingFlags::Public +
			System::Reflection::BindingFlags::NonPublic +
			System::Reflection::BindingFlags::Static +
			System::Reflection::BindingFlags::InvokeMethod +
			System::Reflection::BindingFlags::FlattenHierarchy +
			System::Reflection::BindingFlags::Default
		), nullptr , nullptr , nullptr ));


	
	for (int i = 0; i < <!CollectionName!>Collection->Count; i++)
	{
		BusinessLayerFrameWork::BLCore::BLBusinessBase^ ManagedObject = static_cast<BusinessLayerFrameWork::BLCore::BLBusinessBase^ > (<!CollectionName!>Collection[i]);
		DataKey = static_cast<BusinessLayerFrameWork::BLCore::BLDataKey^> (ManagedObject["DataKey"]);
		System::Type^ DataKeyType = DataKey->GetType();
		
		
		T<!ObjectName!>& UnmanagedObject = * new T<!ObjectName!>();
		char* TmpStr = NULL;
		System::String^ ManagedTmpStr;
		System::Object^ PropValue;
		BusinessLayerFrameWork::BLCore::BLBusinessBase^ RelatedObject;
		
<!ForEachFieldStart(String)!> 
        PropValue = ManagedObject["<!FieldName!>"];
        ManagedTmpStr = static_cast<System::String^> (PropValue);
		TmpStr = new char[ManagedTmpStr->Length + 1];		
		ConversionHelper::ToCharPtr(TmpStr, ManagedTmpStr);
        UnmanagedObject.Set<!FieldName!> (TmpStr);	
		delete[] TmpStr;<!ForEachFieldEnd(String)!> 				
		
<!ForEachFieldStart(NotReferenceValueType)!>
		System::<!FieldManagedType!>^ Tmp<!FieldName!> = dynamic_cast<System::<!FieldManagedType!>^> (ManagedObject["<!FieldName!>"]);
        UnmanagedObject.Set<!FieldName!> (* Tmp<!FieldName!>);<!ForEachFieldEnd(NotReferenceValueType)!> 
		
<!ForEachFieldStart(ReferenceType)!>	
		<!FieldReferenceType!>DataKey* <!FieldName!>DataKey = new <!FieldReferenceType!>DataKey;						
		RelatedObject = static_cast<BusinessLayerFrameWork::BLCore::BLBusinessBase^> 
			(ManagedObject["<!FieldName!>"]);
		System::Int32^ Int<!FieldName!> = dynamic_cast<System::Int32^> (RelatedObject["ID"]);
		<!FieldName!>DataKey->SetID(safe_cast<int> (Int<!FieldName!>));		
		<!FieldReferenceType!>* Tmp<!FieldName!> = <!FieldReferenceType!>::GetObject(* <!FieldName!>DataKey);		
		UnmanagedObject.Set<!FieldName!>(Tmp<!FieldName!>);
		delete <!FieldName!>DataKey;				
<!ForEachFieldEnd(ReferenceType)!> 
		
		UnManagedCollection.Add(UnmanagedObject, true);
	};
};

void Managed_Load<!CollectionName!>FromXML(T<!CollectionName!>& UnManagedCollection)
{

	System::Xml::XmlTextReader^ Reader = gcnew System::Xml::XmlTextReader(DBPath);
	System::Data::DataSet^ NDataSet = gcnew System::Data::DataSet;
	NDataSet->ReadXml(Reader);

	System::Data::DataTableCollection^ TableCollection = NDataSet->Tables;
	System::Data::DataTable^ Table = TableCollection[TableCollection->IndexOf("<!TableName!>")];
	System::Data::DataRowCollection^ Rows = Table->Rows;

	for (int i = 0; i < Table->Rows->Count; i++)
	{
		System::Data::DataRow^ Row = Rows[i];
		T<!ObjectName!>& UnmanagedObject = * new T<!ObjectName!>;
		char* TmpStr = NULL;		


<!ForEachFieldStart(NotReferenceValueType)!>		
		<!FieldType!> Tmp<!FieldName!> = System::Convert::To<!FieldManagedType!>(Row["<!FieldName!><!TableSuffix!>"]);		
        UnmanagedObject.Set<!FieldName!> (Tmp<!FieldName!>);<!ForEachFieldEnd(NotReferenceValueType)!> 

<!ForEachFieldStart(String)!> 
		
		System::<!FieldManagedType!>^ Tmp<!FieldName!>;
		Tmp<!FieldName!> = safe_cast<System::<!FieldManagedType!>^> 
			(Row["<!FieldName!><!TableSuffix!>"]);        
    
		TmpStr = new char[Tmp<!FieldName!>->Length + 1];		
		ConversionHelper::ToCharPtr(TmpStr, Tmp<!FieldName!>);
        UnmanagedObject.Set<!FieldName!> (TmpStr);	
		delete[] TmpStr;<!ForEachFieldEnd(String)!> 				

<!ForEachFieldStart(ReferenceType)!>	
		<!FieldReferenceType!>DataKey* <!FieldName!>DataKey = new <!FieldReferenceType!>DataKey;						
		int Int<!FieldName!> = System::Convert::ToInt32(Row["<!InternalFieldName!><!TableSuffix!>"]);		
		<!FieldName!>DataKey->SetID(Int<!FieldName!>);		
		<!FieldReferenceType!>* Tmp<!FieldName!> = <!FieldReferenceType!>::GetObject(* <!FieldName!>DataKey);		
		UnmanagedObject.Set<!FieldName!>(Tmp<!FieldName!>);
		delete <!FieldName!>DataKey;				
<!ForEachFieldEnd(ReferenceType)!> 

		
		
		UnManagedCollection.Add(UnmanagedObject, true);
	};
};



#pragma managed(pop)

