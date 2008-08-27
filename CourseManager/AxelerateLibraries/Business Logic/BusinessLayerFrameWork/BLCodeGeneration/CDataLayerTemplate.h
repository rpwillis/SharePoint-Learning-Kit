/****************************************************************************
                         Especificaciones Generales
****************************************************************************


Pertenece a:            ServerDataLayerC
Número de Unidad:       <!UnitNumber!>
Nombre de la Unidad:    D20 Characters
Propósito:              Define el objeto T<!ObjectName!>
												
/****************************************************************************
 ****************************************************************************/

#ifndef __<!ObjectName!>_H
#define __<!ObjectName!>_H

#ifdef _UNMANAGEDBUILD
#pragma unmanaged 
#endif


#include "Execs.H"
#include "Strings.H"
#include "ClassFactory.H"
#include "CloneContext.H"


/****************************************************************************
              Constantes de identificacion de objetos (idConst)
****************************************************************************/

#define is<!ObjectName!>						<!ModuleNumber!><!UnitNumber!>00
#define is<!ObjectName!>DataKey					<!ModuleNumber!><!UnitNumber!>01
#define is<!CollectionName!>					<!ModuleNumber!><!UnitNumber!>02
#define is<!ObjectName!>Criteria				<!ModuleNumber!><!UnitNumber!>03

/****************************************************************************
               Punteros a las distintas clases de esta unidad
****************************************************************************/

class T<!ObjectName!>;
class T<!ObjectName!>DataKey;
class T<!CollectionName!>;
class T<!ObjectName!>Criteria;

<!ForEachFieldStart(ReferenceType)!>class <!FieldReferenceType!>; <!ForEachFieldEnd(ReferenceType)!>

typedef  T<!ObjectName!>*						P<!ObjectName!>;
typedef  T<!ObjectName!>DataKey*				P<!ObjectName!>DataKey;
typedef  T<!CollectionName!>*					P<!CollectionName!>;
typedef  T<!ObjectName!>Criteria*				P<!ObjectName!>Criteria;

/****************************************************************************
                      Definición de tipos de la unidad
****************************************************************************/


/****************************************************************************
                           Definición de clases
****************************************************************************/

/************************************************************************
                      OBJETO T<!ObjectName!>DataKey
 ************************************************************************/
// Representa el objeto de llave de datos para <!ObjectName!>.  No se permiten Strings en
// la llave 
class UnmanagedDataLayerLinkType T<!ObjectName!>DataKey : 
	public TRTTIClass<T<!ObjectName!>DataKey, TBaseObject, is<!ObjectName!>DataKey>	
{
protected:
	<!ForEachFieldStart(DataKeyType)!>        <!FieldType!> m_<!FieldName!>; <!ForEachFieldEnd(DataKeyType)!>

public:

	T<!ObjectName!>DataKey();
	
	// Crea un nuevo objeto que copia el objeto toCopy
	T<!ObjectName!>DataKey(T<!ObjectName!>DataKey& toCopy, PBaseObject CloneContext=NULL);
	
	virtual TBaseObject& Clone(PBaseObject CloneContext=NULL);
	
	virtual bool Equals(TBaseObject& BaseObject);


#pragma region "Reflection"

	// Registra los metodos para reflection 
	static void RegisterReflection();
	
<!ForEachFieldStart(DataKeyType)!>    <!FieldType!> Get<!FieldName!>();
	void Set<!FieldName!>(<!FieldType!> Value); <!ForEachFieldEnd(DataKeyType)!> 		
	
#pragma endregion

	//
	~T<!ObjectName!>DataKey();

};

/************************************************************************
                      OBJETO T<!ObjectName!>Criteria
 ************************************************************************/
// Representa un objeto de búsqueda
class UnmanagedDataLayerLinkType T<!ObjectName!>Criteria : 
	public TRTTIClass<T<!ObjectName!>Criteria, TBaseObject, is<!ObjectName!>Criteria>	
{
protected:
<!ForEachFieldStart!>
	<!FieldType!> m_<!FieldName!>; 
	bool m_Search<!FieldName!>; <!ForEachFieldEnd!>

public:

	T<!ObjectName!>Criteria();
	
	// Crea un nuevo objeto que copia el objeto toCopy
	T<!ObjectName!>Criteria(T<!ObjectName!>Criteria& toCopy, PBaseObject CloneContext=NULL);
	
	virtual TBaseObject& Clone(PBaseObject CloneContext=NULL);
	
	virtual bool Equals(TBaseObject& BaseObject);

	virtual bool isMatch(T<!ObjectName!>& BusinessObject);


#pragma region "Reflection"

	// Registra los metodos para reflection 
	static void RegisterReflection();
	
<!ForEachFieldStart!>    <!FieldType!> Get<!FieldName!>();
	void Set<!FieldName!>(<!FieldType!> Value); <!ForEachFieldEnd!> 		
	
#pragma endregion

	//
	~T<!ObjectName!>Criteria();

};
	

	




/************************************************************************
                      OBJETO T<!ObjectName!>
 ************************************************************************/
// Representa el objeto de datos <!ObjectName!> 
class UnmanagedDataLayerLinkType T<!ObjectName!> : 
	public TRTTIClass<T<!ObjectName!>, TBaseObject, is<!ObjectName!>>
{
   protected:
   
<!ForEachFieldStart!>        <!FieldType!> m_<!FieldName!>; <!ForEachFieldEnd!>
		T<!ObjectName!>DataKey m_DataKey;

	static TString m_TableName;
	static TString m_TableSuffix;
   
   public:   

	//
	T<!ObjectName!>();
	
	// Crea un nuevo objeto que copia el objeto toCopy
	T<!ObjectName!>(T<!ObjectName!>& toCopy, PBaseObject CloneContext=NULL);
	
	virtual TBaseObject& Clone(PBaseObject CloneContext=NULL);
	
	// No se crea en memoria dinámica.  No se le debe hacer delete
	T<!ObjectName!>DataKey& GetDataKey();
	
	virtual bool Equals(TBaseObject& BaseObject);

	static char* GetTableName();
	static char* GetTableSuffix();


#pragma region "Reflection"

	// Registra los metodos para reflection 
	static void RegisterReflection();
	
<!ForEachFieldStart(ValueType)!>    <!FieldType!> Get<!FieldName!>();
	void Set<!FieldName!>(<!FieldType!> Value); <!ForEachFieldEnd(ValueType)!> 	

<!ForEachFieldStart(String)!>    <!FieldType!> Get<!FieldName!>();
	void Set<!FieldName!>(<!FieldType!> Value); <!ForEachFieldEnd(String)!> 	
	
	
#pragma endregion

	static T<!ObjectName!>* GetObject(T<!ObjectName!>DataKey& DataKey);

	//
	~T<!ObjectName!>();

};


/************************************************************************
                      OBJETO T<CollectionName>
 ************************************************************************/
// Representa el objeto de datos <CollectionName> 
class T<!CollectionName!> : 
	public TRTTIClass<T<!CollectionName!>, TListGroup, is<!CollectionName!>>
{
   private:
   
   static T<!CollectionName!>* m_CompleteCollection;   
   
   
   public:   

	//No debe usarse, es un sintleton.  Se hace public para reflexión
	T<!CollectionName!>(bool LoadDataFromDB = false);   	
	
#pragma region "Reflection"

	// Registra los metodos para reflection 
	static void RegisterReflection();
#pragma endregion

	static T<!CollectionName!>& GetCollection();
	static T<!CollectionName!>& GetCollection(T<!ObjectName!>Criteria& Criteria);

	//
	~T<!CollectionName!>();

};

#pragma managed(push, on)
void Managed_Load<!CollectionName!>From(T<!CollectionName!>& UnManagedCollection);
void Managed_Load<!CollectionName!>FromXML(T<!CollectionName!>& UnManagedCollection);
#pragma managed(pop)

#endif