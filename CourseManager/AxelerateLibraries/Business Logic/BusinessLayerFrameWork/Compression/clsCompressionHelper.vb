Imports System.IO
Imports System.IO.Stream
Imports System.IO.Compression

Namespace Compression

    Public Class clsCompressionHelper

        ''' <summary>
        ''' Compresses the input buffer
        ''' </summary>
        ''' <param name="UncompressedBuffer"></param>
        ''' <returns>The compressed bytes for the input</returns>
        ''' <remarks></remarks>
        Public Shared Function GZIPCompress(ByVal UncompressedBuffer() As Byte) As Byte()
            'If (Not UncompressedBuffer Is Nothing) AndAlso (UncompressedBuffer.Length > 0) Then
            '    Dim ms As New MemoryStream()

            '    ' Use the newly created memory stream for the compressed data.
            '    Dim compressedZipStream As New GZipStream(ms, CompressionMode.Compress, True)
            '    compressedZipStream.Write(UncompressedBuffer, 0, UncompressedBuffer.Length)
            '    ' Close the stream.
            '    compressedZipStream.Close()
            '    Return ms.GetBuffer()
            'Else
            '    Return UncompressedBuffer
            'End If
            Return UncompressedBuffer
        End Function

        ''' <summary>
        ''' Compresses the input buffer
        ''' </summary>
        ''' <param name="CompressedBuffer"></param>
        ''' <param name="UncompressedFileSize"></param>
        ''' <returns>The compressed bytes for the input</returns>
        ''' <remarks></remarks>
        Public Shared Function GZIPUnCompress(ByVal CompressedBuffer() As Byte, ByVal UncompressedFileSize As Long) As Byte()
            'If (Not CompressedBuffer Is Nothing) AndAlso (CompressedBuffer.Length > 0) Then

            '    Dim ms As New MemoryStream(CompressedBuffer)
            '    Dim compressedZipStream As New GZipStream(ms, CompressionMode.Decompress, True)
            '    Dim decompressedBuffer(CInt(UncompressedFileSize) + 100) As Byte
            '    ' Use the ReadAllBytesFromStream to read the stream.
            '    Dim totalCount As Integer = ReadAllBytesFromStream(compressedZipStream, decompressedBuffer)
            '    compressedZipStream.Close()
            '    Return decompressedBuffer
            'Else
            '    Return CompressedBuffer
            'End If

            Return CompressedBuffer
        End Function

        Public Shared Function ReadAllBytesFromStream(ByVal stream As Stream, ByVal buffer() As Byte) As Integer
            ' Use this method is used to read all bytes from a stream.
            Dim offset As Integer = 0
            Dim totalCount As Integer = 0
            While True
                Dim bytesRead As Integer = stream.Read(buffer, offset, 100)
                If bytesRead = 0 Then
                    Exit While
                End If
                offset += bytesRead
                totalCount += bytesRead
            End While
            Return totalCount
        End Function 'ReadAllBytesFromStream




    End Class
End Namespace