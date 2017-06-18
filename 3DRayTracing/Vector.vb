Public Class Vector
    Private changeX, changeY, changeZ, length, angleX, angleY, angleZ As Double

    Public Sub New(x As Double, y As Double, z As Double)
        Me.changeX = x
        Me.changeY = y
        Me.changeZ = z
        Me.length = Math.Sqrt(x ^ 2 + y ^ 2 + z ^ 2)
        Me.angleX = Math.Acos(changeX / length)
        Me.angleY = Math.Acos(changeY / length)
        Me.angleZ = Math.Acos(changeZ / length)
    End Sub

    Public Function normalize()
        Dim x As Double, y As Double, z As Double
        x = changeX / length
        y = changeY / length
        z = changeZ / length
        Return New Vector(x, y, z)
    End Function

    Public Function add(vect2 As Vector)
        Return New Vector(Me.myX + vect2.myX, Me.myY + vect2.myY, Me.myZ + vect2.myZ)
    End Function

    Public Function subtract(vect2 As Vector)
        Return New Vector(Me.myX - vect2.myX, Me.myY - vect2.myY, Me.myZ - vect2.myZ)
    End Function

    Public Function negative()
        Return New Vector(-1 * Me.myX, -1 * Me.myY, -1 * Me.myZ)
    End Function

    Public Function scale(factor As Double)
        Return New Vector(factor * Me.myX, factor * Me.myY, factor * Me.myZ)
    End Function
    Public Shared Function pt2pt(finish As Point3D, start As Point3D)
        Dim ret As Vector
        ret = New Vector(finish.x - start.x, finish.y - start.y, finish.z - start.z)
        Return ret

    End Function

    Public Function project(ontoVect As Vector)
        Dim vectA = Me.normalize()
        Dim vectB = ontoVect.normalize()
        Dim vectCx, vectCy, vectCz As Double
        Dim vectC As Vector

        vectCx = Vector.dotProduct(vectA, vectB) * vectB.myX()
        vectCy = Vector.dotProduct(vectA, vectB) * vectB.myY()
        vectCz = Vector.dotProduct(vectA, vectB) * vectB.myZ()

        vectC = New Vector(vectCx, vectCy, vectCz)
        Return vectC
    End Function
    Public Shared Function isParallel(vect1 As Vector, vect2 As Vector)
        Dim num = dotProduct(vect1, vect2)
        Dim result = Math.Round(num / (vect1.getLength * vect2.getLength), 3)
        Dim result2 = Math.Round(num / (-1 * vect1.getLength * vect2.getLength), 3)
        If result = 1 Or result2 = 1 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function dotProduct(vect1 As Vector, vect2 As Vector)
        Dim product As Double
        product = vect1.myX * vect2.myX + vect1.myY * vect2.myY + vect1.myZ * vect2.myZ
        Return product
    End Function

    ' i j k i j
    ' x y z x y -> <yr-zq, zp-xr, xq-yp)
    ' p q r p q
    Public Shared Function crossProduct(vect1 As Vector, vect2 As Vector) As Vector
        Dim product As Vector
        product = New Vector((vect1.myY * vect2.myZ - vect1.myZ * vect2.myY), (vect1.myZ * vect2.myX - vect1.myX * vect2.myZ),
            (vect1.myX * vect2.myY - vect1.myY * vect2.myX))
        Return product
    End Function

    Public Property myX()
        Get
            Return changeX
        End Get
        Set(value)
            changeX = value
            Me.length = Math.Sqrt(changeX ^ 2 + changeY ^ 2 + changeZ ^ 2)
            Me.angleX = Math.Acos(changeX / length)
        End Set
    End Property
    Public Property myY()
        Get
            Return changeY
        End Get
        Set(value)
            changeY = value
            Me.length = Math.Sqrt(changeX ^ 2 + changeY ^ 2 + changeZ ^ 2)
            Me.angleY = Math.Acos(changeY / length)
        End Set
    End Property
    Public Property myZ()
        Get
            Return changeZ
        End Get
        Set(value)
            changeY = value
            Me.length = Math.Sqrt(changeX ^ 2 + changeY ^ 2 + changeZ ^ 2)
            Me.angleZ = Math.Acos(changeZ / length)
        End Set
    End Property

    Public Function getAngleX()
        Return angleX
    End Function
    Public Function getAngleY()
        Return angleY
    End Function
    Public Function getAngleZ()
        Return angleZ
    End Function
    Public Function getLength()
        Return length
    End Function

    Public Overrides Function ToString() As String
        Return "<" + CStr(Math.Round(changeX, 4)) + "," + CStr(Math.Round(changeY, 4)) + "," + CStr(Math.Round(changeZ, 4)) + ">"
    End Function
End Class
