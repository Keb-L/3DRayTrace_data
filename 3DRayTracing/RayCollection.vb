Public Class RayCollection
    Private col As List(Of Ray)

    Public Sub New(r1 As Ray)
        col = New List(Of Ray)(5)
        col.Add(r1)
    End Sub

    Public Sub addRay(r1 As Ray)
        col.Add(r1)
    End Sub

    Public Sub changeray(index As Integer, r1 As Ray)
        col(index) = r1
    End Sub

    Public Overrides Function toString() As String
        Dim str As String = ""
        For Each Ray In col
            str += Ray.ToString
        Next
        Return str
    End Function
End Class
