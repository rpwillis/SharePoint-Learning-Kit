Imports System.Security.Cryptography
Imports System.Xml

Namespace BLCore.Security

    Public Class AsymmetricEncryption

        Private Shared m_RSAPublic As RSAParameters = Nothing
        Private Shared m_RSAPublicLoaded As Boolean = False

        Private Shared m_RSAPair As RSAParameters = Nothing
        Private Shared m_RSAPairLoaded As Boolean = False




        Public Shared Function Encrypt(ByVal DataToEncrypt As String, ByVal RSAKeyInfo As RSAParameters) As String
            Try
                'Crea una nueva instancia de RSACryptoServiceProvider.
                Dim RSA As RSACryptoServiceProvider = New RSACryptoServiceProvider
                Dim BytesToEncrypt As Byte() = System.Text.Encoding.Default.GetBytes(DataToEncrypt)

                'Importa la informacion de la llave publica RSA. 
                RSA.ImportParameters(RSAKeyInfo)
                'Encripta la informacion DataToEncrypt y especifica el OAEP padding
                'OAEP padding esta solo disponible para Windows XP o superior
                'OAEP padding protege contra tipos especificos de ataque
                'Return RSA.Encrypt(DataToEncrypt, DoOAEPPadding) como byte()
                Dim DoOAEPPaddingString As String
                DoOAEPPaddingString = System.Configuration.ConfigurationSettings.AppSettings("OAEPPadding").ToUpper
                Dim DoOAEPPadding As Boolean = False
                If DoOAEPPaddingString = "TRUE" Then
                    DoOAEPPadding = True
                End If

                Return System.Text.Encoding.Default.GetString(RSA.Encrypt(BytesToEncrypt, DoOAEPPadding))
            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_EncryptError, ex)

                Return Nothing
            End Try
        End Function


        Public Shared Function Decrypt(ByVal DataToDecrypt As String, ByVal RSAKeyInfo As RSAParameters) As String
            Try
                If DataToDecrypt = "" Then
                    Return ""
                End If
                'Crea una nueva instancia de RSACryptoServiceProvider.
                Dim RSA As RSACryptoServiceProvider = New RSACryptoServiceProvider
                Dim BytesToDecrypt As Byte() = System.Text.Encoding.Default.GetBytes(DataToDecrypt)
                'Importa la informacion de la llave privada RSA. 
                RSA.ImportParameters(RSAKeyInfo)
                'Desencripta la informacion DataToDecrypt y especifica el OAEP padding
                'OAEP padding esta solo disponible para Windows XP o superior
                'OAEP padding protege contra tipos especificos de ataque
                'Return RSA.Decrypt(DataToDecrypt, DoOAEPPadding) como byte()
                Dim DoOAEPPaddingString As String
                DoOAEPPaddingString = System.Configuration.ConfigurationSettings.AppSettings("OAEPPadding").ToUpper
                Dim DoOAEPPadding As Boolean = False
                If DoOAEPPaddingString = "TRUE" Then
                    DoOAEPPadding = True
                End If

                Return System.Text.Encoding.Default.GetString(RSA.Decrypt(BytesToDecrypt, DoOAEPPadding)) 'como string


            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_DecryptError, ex)
                Return Nothing
            End Try
        End Function
        Public Shared Function GenerateKeys() As RSACryptoServiceProvider
            Try


                'Crea una nueva instancia de RSACryptoServiceProvider para generar las llaves
                Dim RSA As RSACryptoServiceProvider = New RSACryptoServiceProvider
                Return RSA


                'Dim a As New Xml.XmlDocument
                'a.LoadXml(XMLString)
                'Dim b As New IO.FileStream("Nombre.xmL", IO.FileMode.Create)
                'a.Save(b)

            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_EncryptionKeyGeneration, ex)
                Return Nothing
            End Try
        End Function

        Public Shared Function GetPublicKey() As RSAParameters
            If Not m_RSAPublicLoaded Then
                Try
                    Dim XMLFileName As String
                    Try
                        XMLFileName = System.Configuration.ConfigurationSettings.AppSettings("XMLPath").ToUpper + "\PublicKey.xml"
                    Catch ex As Exception
                        XMLFileName = System.Windows.Forms.Application.StartupPath + "\..\XML\PublicKey.xml"
                    End Try

                    Dim FilePublicKey As New IO.FileStream(XMLFileName, IO.FileMode.Open)
                    ' Dim FilePublicKey As New IO.FileStream("../XML/PublicKey.xml", IO.FileMode.Open)
                    Dim XmlDocumentPublicKey As New XmlDocument
                    XmlDocumentPublicKey.Load(FilePublicKey)
                    Dim PublicKeyString As String
                    PublicKeyString = XmlDocumentPublicKey.InnerXml()
                    Dim RSA As RSACryptoServiceProvider = New RSACryptoServiceProvider
                    RSA.FromXmlString(PublicKeyString)
                    FilePublicKey.Close()
                    m_RSAPublic = RSA.ExportParameters(False)
                    m_RSAPublicLoaded = True
                Catch ex As Exception
                    m_RSAPublicLoaded = False
                    Throw New System.Exception(BLResources.Exception_PublicKeyError, ex)
                End Try
            End If

            Return m_RSAPublic
        End Function

        Public Shared Function GetPairKeys() As RSAParameters
            If Not m_RSAPairLoaded Then
                Try
                    Dim XMLFileName As String
                    Try
                        XMLFileName = System.Configuration.ConfigurationSettings.AppSettings("XMLPath").ToUpper + "\PairKeys.xml"
                    Catch ex As Exception
                        XMLFileName = System.Windows.Forms.Application.StartupPath + "\..\XML\PairKeys.xml"
                    End Try

                    Dim FilePairKeys As New IO.FileStream(XMLFileName, IO.FileMode.Open) '("../XML/PairKeys.xml", IO.FileMode.Open)
                    Dim XmlDocumentPairKeys As New XmlDocument
                    XmlDocumentPairKeys.Load(FilePairKeys)
                    Dim PairKeysString As String
                    PairKeysString = XmlDocumentPairKeys.InnerXml()
                    Dim RSA As RSACryptoServiceProvider = New RSACryptoServiceProvider
                    RSA.FromXmlString(PairKeysString)
                    FilePairKeys.Close()
                    m_RSAPair = RSA.ExportParameters(True)
                    m_RSAPairLoaded = True
                Catch ex As Exception
                    m_RSAPairLoaded = False
                    Throw New System.Exception(BLResources.Exception_KeyPairError, ex)
                End Try
            End If
            Return m_RSAPair
        End Function

        'Inicializa los valores por defecto de la clase
        Private Sub New()

        End Sub
        Public Shared Function NewAsymmetricEncryption() As AsymmetricEncryption
            'Return CType(DataPortal.Create(New Criteria(0)), BOUsuario)
            Return New AsymmetricEncryption
        End Function
    End Class
End Namespace