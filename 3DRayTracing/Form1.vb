
Imports System.IO

Public Class Form1
    Private RCList As List(Of RayCollection)
    Private nVect = New Vector(0, 0, -1)
    Private nVect2 = New Vector(0, 0, -1)
    Private orthVect = New Vector(0, 0, -1)
    Private plnVect = New Vector(0, 0, -1)
    Private Lens1 As Sphere
    Private Lens2 As Sphere
    Private Lens3 As Sphere

    Dim samples, validCt1, validCt2 As Double
    Dim total1, total2 As Double
    Dim ctrlL, ctrlL2, refAb(1), refBe(1), refMin(1), refMax(1) As Double
    Private coreRad As Double

    Dim origin As Point3D
    Dim centerTheta1, centerTheta2 As Double
    Dim centerRay1, centerRay2 As Ray
    Dim centerIntersect1, centerIntersect2 As Point3D

    Dim factor = 1.1

    Dim outerNA As Double
    Dim innerNA As Double

    Dim apeture As Double

    Dim counterFalse, counterOoR As Integer

    Dim aptArr(1), phiArr(1), coreRadArr(1), thetaArr(1) As Double

    'Data Collection purposes
    Dim lengths(3, 1) As Double



    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        samples = numSample.Value
        RCList = New List(Of RayCollection)(samples)
        Dim ctrl As Object
        For Each ctrl In GroupBox1.Controls
            If TypeOf ctrl Is NumericUpDown Then
                Dim ctl As NumericUpDown = ctrl

                'AddHandler ctl.ValueChanged, AddressOf param_ValueChanged
            End If
        Next
        recalculate()

        StatusNotif.Text = "Working..."
        Me.Update()

        Call UpdateForm()

        StatusNotif.Text = "Complete..."
    End Sub
    Private Sub drawCircle(pt As Point3D)
        Dim rad
        For i = 0 To 360 Step 2
            rad = i / 180 * Math.PI
            RefDist.Series("Circle").Points.AddXY(pt.x + coreRad * Math.Cos(rad), pt.y + coreRad * Math.Sin(rad))
        Next
    End Sub

    Private Sub param_ValueChanged(ByVal sender As Object, ByVal e As EventArgs)
        StatusNotif.Text = "Working..."
        Me.Update()

        UpdateForm()

        StatusNotif.Text = "Complete..."
    End Sub

    Private Sub recalculate()
        ReDim aptArr(numSample.Value), phiArr(numSample.Value), coreRadArr(numSample.Value), thetaArr(numSample.Value)
        Dim rng = New Random
        Dim rand = New Random
        For i = 1 To numSample.Value - 1
            'AptArr and PhiArr are for selection of output angle
            aptArr(i) = CDbl((rand.Next(apeture * 1000) + 1) / 1000) 'aperture *1000 choices
            If rng.Next(0, 2) = 0 Then
                aptArr(i) *= -1
            End If
            'phiArr(i) = rand.Next(361) / 180 * Math.PI ' 360 choices
            phiArr(i) = rand.Next(181) / 180 * Math.PI ' 180 choices

            'coreRadArr and thetaArr are for selection on the 2D circle/end of the wire
            coreRadArr(i) = CDbl((rand.Next(coreRad * 10000 + 1) / 10000)) ' coreRad * 10 choices
            thetaArr(i) = CDbl(rand.Next(361) / 180 * Math.PI) ' 360 choices angle on x-y plane
        Next
        lengths = New Double(3, numSample.Value - 1) {}

    End Sub

    Private Sub UpdateForm()

        recalculate()

        Try
            resetSeriesPt()

            coreRad = bxCoreRad.Value / 1000
            samples = numSample.Value
            'Creating Lens1
            Lens1 = New Sphere(New Point3D(bxL1X.Value, bxL1Y.Value, bxL1Z.Value), bxL1R.Value)
            'Creating Lens2
            Lens2 = New Sphere(New Point3D(bxL2X.Value, bxL2Y.Value, bxL2Z.Value), bxL2R.Value)
            'Creating Lens3
            Lens3 = New Sphere(New Point3D(bxL3X.Value, bxL3Y.Value, bxL3Z.Value), bxL3R.Value)

            'Populating RayCollections
            '   y
            '   |
            '   o
            ' /   \
            'z     x
            origin = New Point3D(bxRX.Value, bxRY.Value, bxRZ.Value)
            apeture = bxRA.Value
            outerNA = apeture
            innerNA = snellLaw(outerNA, bxN1.Value, bxN2.Value)
            Dim length As Double

            Dim rand = New Random()
            'Dim theta, phi, randR, randT As Double


            Dim r1, r2, r3, r4, r5, o1, o2, o3 As Ray
            Dim i1, i2, i3 As Point3D
            Dim normal, normal2, normal3 As Vector
            Dim retInt1, retInt2 As Point3D
            Dim angleIncid, transAngle As Decimal
            Dim intersect As Point3D

            Dim r1_angle = InitAngle.Value

            'v = <sin theta * cos phi, sin theta * sin phi, cos theta)
            'r1 = New Ray(origin, New Vector(Math.Cos(r1_angle), Math.Sin(r1_angle), 1))
            r1 = New Ray(origin, New Vector(0, Math.Sin(r1_angle), Math.Cos(r1_angle)))

            i1 = findIntersect(r1, Lens1)
            r1 = New Ray(origin, Vector.pt2pt(i1, origin))

            o1 = New Ray(i1, Vector.pt2pt(i1, Lens1.myOrigin))
            normal = Vector.crossProduct(r1.myVector, o1.myVector)

            angleIncid = calcIncidence(r1.myVector, o1.myVector)
            transAngle = snellLaw(angleIncid, bxN1.Value, bxN2.Value)

            'reflect
            r2 = reflect(r1, i1, o1, normal)
            retInt1 = r2.axisIntersectionZ(r1.myOrigin.z)
            r2 = New Ray(i1, Vector.pt2pt(retInt1, i1))

            nVect = r2.myVector ' Settings Reference Vector 

            centerTheta1 = snellLaw(findAngle2(r2, orthVect), 1, 1.5)
            bxFbrAngR.Text = Math.Round(centerTheta1, 8)

            intersect = Point3D.addVector(r2.myVector, r2.myOrigin)
            centerRay1 = refract(r2, intersect, New Ray(intersect, orthVect), plnVect, findAngle2(r2, orthVect), snellLaw(findAngle2(r2, orthVect), 1, 1.5))
            centerIntersect1 = retInt1

            EmiDist.Series("Normal").Points.AddXY(origin.x, origin.y)
            EmiDist.Series("Normal").Points.Item(EmiDist.Series("Normal").Points.Count - 1).ToolTip =
            EmiDist.Series("Normal").Points.Item(EmiDist.Series("Normal").Points.Count - 1).ToString

            RefDist.Series("Normal").Points.AddXY(retInt1.x, retInt1.y)
            RefDist.Series("Normal").Color = Color.LimeGreen
            RefDist.Series("All").Points.AddXY(retInt1.x, retInt1.y)


            r3 = refract(r1, i1, o1, normal, angleIncid, transAngle)

            i2 = findIntersect(r3, Lens2)
            r3 = New Ray(i1, Vector.pt2pt(i2, i1))

            o2 = New Ray(i2, Vector.pt2pt(i2, Lens2.myOrigin))
            normal2 = Vector.crossProduct(r3.myVector, o2.myVector)

            r4 = reflect(r3, i2, o2, normal2)
            i3 = findIntersect(New Ray(r4.myOrigin, r4.myVector.negative()), Lens3)
            r4 = New Ray(i2, Vector.pt2pt(i3, i2))

            o3 = New Ray(i3, Vector.pt2pt(Lens3.myOrigin, i3))
            normal3 = Vector.crossProduct(r4.myVector, o3.myVector)

            angleIncid = calcIncidence(r4.myVector, o3.myVector)
            transAngle = snellLaw(angleIncid, bxN2.Value, bxN1.Value)

            r5 = refract(r4, i3, o3, normal3, angleIncid, transAngle)
            retInt2 = r5.axisIntersectionZ(r1.myOrigin.z)
            r5 = New Ray(i3, Vector.pt2pt(i3, retInt2))

            nVect2 = r5.myVector

            centerTheta2 = snellLaw(findAngle2(r5, orthVect), 1, 1.5)
            bxFbrAngT.Text = Math.Round(centerTheta2, 8)
            intersect = Point3D.addVector(r5.myVector, r5.myOrigin)
            centerRay2 = refract(r5, intersect, New Ray(intersect, orthVect), plnVect, findAngle2(r5, orthVect), snellLaw(findAngle2(r5, orthVect), 1, 1.5))
            centerIntersect2 = retInt2

            Port1.Text = origin.toString
            Port2.Text = retInt1.toString
            Port3.Text = retInt2.toString

            ctrlL = r1.myVector.getLength * bxN1.Value + r2.myVector.getLength * bxN1.Value
            ctrlL2 = r1.myVector.getLength * bxN1.Value + r3.myVector.getLength * bxN2.Value + r4.myVector.getLength * bxN2.Value + r5.myVector.getLength * bxN1.Value
            total1 = ctrlL
            total2 = ctrlL2

            lengths(0, 0) = ctrlL
            lengths(2, 0) = ctrlL2

            'bxCtrR.Text = Math.Round(ctrlL, 8).ToString
            'bxCtrT.Text = Math.Round(ctrlL2, 8).ToString

            TranDist.Series("Normal").Points.AddXY(retInt2.x, retInt2.y)

            OutToRef.Series("Normal").Points.AddXY(0, ctrlL)
            OutToTran.Series("Normal").Points.AddXY(0, ctrlL2)
            OutToRef.Series("All").Points.AddXY(0, ctrlL)
            OutToTran.Series("All").Points.AddXY(0, ctrlL2)

            'resetTxtBx()


            For i = 1 To samples - 1
                'v = <sin theta * cos phi, sin theta * sin phi, cos theta)

                ' phi = 0
                'coreRadArr is the select of the circle.
                Dim cent As Point3D = New Point3D(origin.x + coreRadArr(i) * Math.Cos(thetaArr(i)), ' randT
                                                  origin.y + coreRadArr(i) * Math.Sin(thetaArr(i)),
                                                  origin.z)

                'r1 = New Ray(cent, New Vector(Math.Cos(r1_angle) + Math.Sin(aptArr(i)) * Math.Cos(phiArr(i)),
                '                              Math.Sin(r1_angle) + Math.Sin(aptArr(i)) * Math.Sin(phiArr(i)),
                '                              Math.Cos(aptArr(i))))


                r1 = New Ray(cent, New Vector(Math.Cos(r1_angle + aptArr(i)) * Math.Sin(phiArr(i)),
                                              Math.Sin(r1_angle + aptArr(i)),
                                              Math.Cos(r1_angle + aptArr(i)) * Math.Cos(phiArr(i))))

                'Console.WriteLine(r1.ToString)
                EmiDist.Series("Valid").Points.AddXY(cent.x, cent.y)
                i1 = findIntersect(r1, Lens1)

                r1 = New Ray(cent, Vector.pt2pt(i1, cent))

                o1 = New Ray(i1, Vector.pt2pt(i1, Lens1.myOrigin))
                normal = Vector.crossProduct(r1.myVector, o1.myVector)

                angleIncid = calcIncidence(r1.myVector, o1.myVector)
                transAngle = snellLaw(angleIncid, bxN1.Value, bxN2.Value)
                'reflect
                r2 = reflect(r1, i1, o1, normal)

                retInt1 = r2.axisIntersectionZ(r1.myOrigin.z)
                r2 = New Ray(r2.myOrigin, Vector.pt2pt(retInt1, r2.myOrigin))

                length = r1.myVector.getLength * bxN1.Value + r2.myVector.getLength * bxN1.Value

                If refractFiber(r2, retInt1, centerRay1, centerIntersect1, bxN1.Value, bxN2.Value) Then

                    RefDist.Series("Valid").Points.AddXY(retInt1.x, retInt1.y)
                    RefDist.Series("Valid").Points.Item(RefDist.Series("Valid").Points.Count - 1).ToolTip =
                     RefDist.Series("Valid").Points.Item(RefDist.Series("Valid").Points.Count - 1).ToString

                    RefDist.Series("All").Points.AddXY(retInt1.x, retInt1.y)

                    OutToRef.Series("Normal").Points.AddXY(i, length)
                    OutToRef.Series("All").Points.AddXY(i, length)
                    evalLength(0, length)
                    If (btnLength.Checked) Then
                        lengths(0, i) = length
                    End If

                Else
                    RefDist.Series("Invalid").Points.AddXY(retInt1.x, retInt1.y)
                    RefDist.Series("Invalid").Points.Item(RefDist.Series("Invalid").Points.Count - 1).ToolTip =
                        RefDist.Series("Invalid").Points.Item(RefDist.Series("Invalid").Points.Count - 1).ToString
                    RefDist.Series("All").Points.AddXY(retInt1.x, retInt1.y)

                    If (btnLength.Checked) Then
                        lengths(1, i) = length
                    End If
                End If

                'rotate
                r3 = refract(r1, i1, o1, normal, angleIncid, transAngle)

                i2 = findIntersect(r3, Lens2)
                r3 = New Ray(i1, Vector.pt2pt(i2, i1))

                o2 = New Ray(i2, Vector.pt2pt(i2, Lens2.myOrigin))
                normal2 = Vector.crossProduct(r3.myVector, o2.myVector)

                r4 = reflect(r3, i2, o2, normal2)
                i3 = findIntersect(New Ray(r4.myOrigin, r4.myVector.negative()), Lens3)
                r4 = New Ray(i2, Vector.pt2pt(i3, i2))

                o3 = New Ray(i3, Vector.pt2pt(Lens3.myOrigin, i3))
                normal3 = Vector.crossProduct(r4.myVector, o3.myVector)


                angleIncid = calcIncidence(r4.myVector, o3.myVector)
                transAngle = snellLaw(angleIncid, bxN2.Value, bxN1.Value)

                r5 = refract(r4, i3, o3, normal3, angleIncid, transAngle)
                retInt2 = r5.axisIntersectionZ(r1.myOrigin.z)

                r5 = New Ray(i3, Vector.pt2pt(i3, retInt2))

                length = r1.myVector.getLength * bxN1.Value + r3.myVector.getLength * bxN2.Value + r4.myVector.getLength * bxN2.Value + r5.myVector.getLength * bxN1.Value

                If refractFiber(r5, retInt2, centerRay2, centerIntersect2, bxN1.Value, bxN2.Value) Then

                    TranDist.Series("Valid").Points.AddXY(retInt2.x, retInt2.y)
                    TranDist.Series("Valid").Points.Item(TranDist.Series("Valid").Points.Count - 1).ToolTip =
                        TranDist.Series("Valid").Points.Item(TranDist.Series("Valid").Points.Count - 1).ToString

                    TranDist.Series("All").Points.AddXY(retInt2.x, retInt2.y)

                    OutToTran.Series("Normal").Points.AddXY(i, length)
                    OutToTran.Series("All").Points.AddXY(i, length)
                    evalLength(1, length)

                    If (btnLength.Checked) Then
                        lengths(2, i) = length
                    End If

                Else
                    TranDist.Series("Invalid").Points.AddXY(retInt2.x, retInt2.y)
                    TranDist.Series("Invalid").Points.Item(TranDist.Series("Invalid").Points.Count - 1).ToolTip =
                    TranDist.Series("Invalid").Points.Item(TranDist.Series("Invalid").Points.Count - 1).ToString
                    TranDist.Series("All").Points.AddXY(retInt2.x, retInt2.y)

                    If (btnLength.Checked) Then
                        lengths(3, i) = length
                    End If

                End If
            Next
            validCt1 = OutToRef.Series("Normal").Points.Count
            validCt2 = OutToTran.Series("Normal").Points.Count
            Dim yArr(validCt1 - 1) As Decimal

            For i = 0 To validCt1 - 1
                yArr(i) = OutToRef.Series("Normal").Points.Item(i).YValues(0)
            Next
            Array.Sort(yArr)
            refMin(0) = yArr(0)
            refMax(0) = yArr(yArr.Length - 1)

            ReDim yArr(validCt2 - 1)
            For i = 0 To validCt2 - 1
                yArr(i) = OutToTran.Series("Normal").Points.Item(i).YValues(0)
            Next
            Array.Sort(yArr)
            refMin(1) = yArr(0)
            refMax(1) = yArr(yArr.Length - 1)

            'updating Stats
            'updateStatBoxes(0)
            'updateStatBoxes(1)

            ctValid.Text = CStr(RefDist.Series("Valid").Points.Count + 1)
            ctInval.Text = CStr(RefDist.Series("Invalid").Points.Count)
            ctValidT.Text = CStr(TranDist.Series("Valid").Points.Count + 1)
            ctInvalT.Text = CStr(TranDist.Series("Invalid").Points.Count)
            mnuAutoScale_Click(Nothing, Nothing)

            bxInvAng.Text = counterFalse.ToString
            bxOoR.Text = counterOoR.ToString

        Catch ex As OverflowException
            MsgBox("An OverflowException has occurred! Please ensure that the parameters are valid.", vbExclamation, "Error!")
        End Try
    End Sub

    Private Function rotatePoint(angle As Double, pln As Plane, refRay As Ray)
        Dim axis = New Vector(1, 0, 0)
        Dim pt As Point3D = findIntersectPln(refRay, pln) ' intersection point on plane
        Console.WriteLine(pt)
        Dim ptDif = Vector.pt2pt(pln.intersect, pt) 'Vector between intersection pt and normal pt
        Dim x, y, z, u, v, w As Double
        x = ptDif.myx
        y = ptDif.myy
        z = ptDif.myz

        u = axis.myX
        v = axis.myY
        w = axis.myZ

        Dim outR, outQ, outP As Double
        outP = (u * (u * x + v * y + z * w) * (1 - Math.Cos(angle)) + (u ^ 2 + w ^ 2 + v ^ 2) * x * Math.Cos(angle) +
                  Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-w * y + v * z) * Math.Sin(angle)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outQ = (v * (u * x + v * y + z * w) * (1 - Math.Cos(angle)) + (u ^ 2 + w ^ 2 + v ^ 2) * y * Math.Cos(angle) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (w * x - u * z) * Math.Sin(angle)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outR = (w * (u * x + v * y + z * w) * (1 - Math.Cos(angle)) + (u ^ 2 + w ^ 2 + v ^ 2) * z * Math.Cos(angle) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-v * x + u * y) * Math.Sin(angle)) / (u ^ 2 + v ^ 2 + w ^ 2)

        Dim ret = New Point3D(outP, outQ, outR)

        Return ret
    End Function

    'Private Sub updateStatBoxes(index As Integer)
    '    Select Case index
    '        Case 0
    '            Dim stdev = findstDev(OutToRef.Series("Normal").Points.ToArray(), ctrlL, OutToRef)
    '            bxRefStatDev.Text = CStr(Math.Round(stdev, 8))
    '            If StDevHist.Checked Then
    '                histogram(OutToRef.Series("Normal").Points.ToArray, OutToRef, stdev, total1 / validCt1)
    '            Else
    '                histogram2(OutToRef.Series("Normal").Points.ToArray, OutToRef)
    '            End If

    '            bxRefStatAb.Text = CStr(refAb(0))
    '            bxRefStatBe.Text = CStr(refBe(0))
    '            bxRefStatMax.Text = CStr(Math.Round(refMax(0), 8))
    '            bxRefStatMin.Text = CStr(Math.Round(refMin(0), 8))
    '            bxRefStatRa.Text = CStr(Math.Round(refMax(0) - refMin(0), 8))
    '            bxRefStatAvg.Text = CStr(Math.Round(total1 / validCt1, 8))
    '            bxRefStatADf.Text = CStr(Math.Round(total1 / validCt1 - ctrlL, 8))
    '            Dim mid = findMedian(OutToRef.Series(0).Points.ToArray(), ctrlL, OutToRef)

    '            bxRefStatMed.Text = CStr(Math.Round(mid, 8))
    '            bxRefStatMDf.Text = CStr(Math.Round(mid - ctrlL, 8))

    '            OutToRef.Series("Central").Points.AddXY(0, ctrlL)
    '            OutToRef.Series("Central").Points.AddXY(OutToRef.ChartAreas(0).AxisY.Maximum, ctrlL)
    '            OutToRef.Series("Average").Points.AddXY(0, total1 / validCt1)
    '            OutToRef.Series("Average").Points.AddXY(OutToRef.ChartAreas(0).AxisY.Maximum, total1 / validCt1)
    '        Case 1
    '            Dim stdev = findstDev(OutToTran.Series("Normal").Points.ToArray(), ctrlL2, OutToTran)
    '            bxTranStatDev.Text = CStr(Math.Round(stdev, 8))

    '            If StDevHist.Checked Then
    '                histogram(OutToTran.Series("Normal").Points.ToArray, OutToTran, stdev, total2 / validCt2)
    '            Else
    '                histogram2(OutToTran.Series("Normal").Points.ToArray, OutToTran)
    '            End If
    '            bxTranStatAb.Text = CStr(refAb(1))
    '            bxTranStatBe.Text = CStr(refBe(1))
    '            bxTranStatMax.Text = CStr(Math.Round(refMax(1), 8))
    '            bxTranStatMin.Text = CStr(Math.Round(refMin(1), 8))
    '            bxTranStatRa.Text = CStr(Math.Round(refMax(1) - refMin(1), 8))
    '            bxTranStatAvg.Text = CStr(Math.Round(total2 / validCt2, 8))
    '            bxTranStatADf.Text = CStr(Math.Round(total2 / validCt2 - ctrlL2, 8))
    '            Dim mid = findMedian(OutToTran.Series(0).Points.ToArray(), ctrlL2, OutToTran)

    '            bxTranStatMed.Text = CStr(Math.Round(mid, 8))
    '            bxTranStatMDf.Text = CStr(Math.Round(mid - ctrlL2, 8))

    '            OutToTran.Series("Central").Points.AddXY(0, ctrlL2)
    '            OutToTran.Series("Central").Points.AddXY(OutToTran.ChartAreas(0).AxisY.Maximum, ctrlL2)
    '            OutToTran.Series("Average").Points.AddXY(0, total2 / validCt2)
    '            OutToTran.Series("Average").Points.AddXY(OutToTran.ChartAreas(0).AxisY.Maximum, total2 / validCt2)
    '    End Select



    'End Sub

    Private Sub evalLength(index As Integer, length As Double)
        Dim o As Double
        Select Case index
            Case 0
                total1 += length
                o = ctrlL
            Case 1
                total2 += length
                o = ctrlL2
        End Select

        If (length > o) Then
            refAb(index) += 1
        ElseIf (length < o) Then
            refBe(index) += 1
        End If
    End Sub

    Private Function findstDev(arr As System.Windows.Forms.DataVisualization.Charting.DataPoint(), reference As Double,
                          targ As System.Windows.Forms.DataVisualization.Charting.Chart)
        If arr.Length = 0 Then
            Return Nothing
        End If
        Dim statArr(arr.Length) As Decimal
        Dim sum, avg, var As Decimal
        For i = 0 To arr.Length - 1
            statArr(i) = arr(i).YValues(0)
            sum += arr(i).YValues(0)
        Next
        statArr(arr.Length) = reference
        sum += reference
        Array.Sort(statArr)
        avg = sum / (arr.Length + 1)

        For i = 0 To arr.Length - 1
            var += (statArr(i) - avg) ^ 2
        Next
        var /= arr.Length
        Dim stDev = Math.Sqrt(var)


        '        For i = -3 To 3
        '            If i = 0 Then GoTo skip
        '            targ.Series("StDev").Points.AddXY(0, avg + stDev * i)
        '            targ.Series("StDev").Points.AddXY(samples, avg + stDev * i)
        'skip:
        '        Next
        Return stDev
    End Function
    Private Function findMedian(arr As System.Windows.Forms.DataVisualization.Charting.DataPoint(), reference As Double,
                           targ As System.Windows.Forms.DataVisualization.Charting.Chart)
        Dim statArr(arr.Length) As Double
        For i = 0 To arr.Length - 1
            statArr(i) = arr(i).YValues(0)
        Next
        statArr(arr.Length) = reference
        Array.Sort(statArr)
        Dim mid As Double
        If statArr.Length Mod 2 = 1 Then
            mid = statArr((statArr.Length - 1) / 2)
        Else
            mid = (statArr(statArr.Length / 2 - 1) + statArr(statArr.Length / 2)) / 2
        End If
        targ.Series("Median").Points.AddXY(0, mid)
        targ.Series("Median").Points.AddXY(targ.ChartAreas(0).AxisY.Maximum, mid)

        Return mid
    End Function
    Private Function calcIncidence(r1 As Vector, r2 As Vector)
        Return Math.Acos(Vector.dotProduct(r2, r1) / (r1.getLength * r2.getLength))
    End Function
    Private Function quad(a As Double, b As Double, c As Double) As Double
        Dim d As Double = b ^ 2 - 4 * a * c
        Dim ret = (-b + Math.Sqrt(d)) / (2 * a)
        'If ret < incomingRay.myOrigin.xCoordinate Then
        '    ret = (-b - Math.Sqrt(d)) / (2 * a)
        'End If
        Return ret
    End Function
    Private Function findIntersect(incomingRay As Ray, Lens As Sphere) As Point3D
        Dim x, y, z, x1, y1, z1, xc, yc, zc, vx, vy, vz, a, b, c, t As Double
        x1 = incomingRay.myOrigin.x
        y1 = incomingRay.myOrigin.y
        z1 = incomingRay.myOrigin.z
        xc = Lens.myOrigin.x
        yc = Lens.myOrigin.y
        zc = Lens.myOrigin.z
        vx = incomingRay.myVector.myX
        vy = incomingRay.myVector.myY
        vz = incomingRay.myVector.myZ
        a = vx ^ 2 + vy ^ 2 + vz ^ 2
        b = 2 * (vx * (x1 - xc) + vy * (y1 - yc) + vz * (z1 - zc))
        c = (x1 - xc) ^ 2 + (y1 - yc) ^ 2 + (z1 - zc) ^ 2 - Lens.myRadius ^ 2
        If a = 0 Then
            t = -c / b
        Else
            t = quad(a, b, c)
        End If

        x = x1 + t * (vx)
        y = y1 + t * (vy)
        z = z1 + t * (vz)
        Dim intersect = New Point3D(x, y, z)
        Return intersect
    End Function

    ''' <summary>
    ''' Takes a Ray (Point and Vector) and a Plane(Point and Normal) and determines where the Ray intersects the plane.
    ''' </summary>
    ''' <param name="incomingRay"></param>
    ''' <param name="plane"></param>
    ''' <returns></returns>
    Private Function findIntersectPln(incomingRay As Ray, plane As Plane) As Point3D
        'http://geomalgorithms.com/a05-_intersect-1.html
        Dim P0 = incomingRay.myOrigin
        Dim V0 = plane.intersect
        Dim n = plane.norm
        Dim s As Double ' scalar
        Dim u As Vector = incomingRay.myVector
        Dim w = Vector.pt2pt(P0, V0)

        s = Vector.dotProduct(n.negative(), w) / Vector.dotProduct(n, u)

        'New point
        Dim newVect = u.scale(s)
        Dim endPoint = Point3D.addVector(newVect, P0)

        If plane.isOnPlane(endPoint) Then
            Return endPoint
        Else
            Console.WriteLine("Error. Point Is Not On plane")
            Return Nothing
        End If

    End Function

    Private Sub bxBinCt_ValueChanged(sender As Object, e As EventArgs) Handles bxBinCt.ValueChanged
        chgGraphStyle_CheckedChanged(sender, e)
    End Sub

    ''' <summary>
    ''' Takes an incoming Ray, intersection point, orthogonal ray and normal ray as parameters. Performs a rotation about an arbitrary axis (normal) to determine 
    ''' the reflected ray. 
    ''' </summary>
    ''' <param name="r1"></param>
    ''' <param name="intersect"></param>
    ''' <param name="o1"></param>
    ''' <param name="norm"></param>
    ''' <returns>Returns a Ray with the same angle of incidence as the incoming Ray</returns>
    Private Function reflect(r1 As Ray, intersect As Point3D, o1 As Ray, norm As Vector) As Ray
        Dim outRay As Ray ' Return Ray 

        Dim angleIncid = Math.Acos(Vector.dotProduct(r1.myVector, o1.myVector) / (r1.myVector.getLength() * o1.myVector.getLength()))
        If angleIncid = 0 Then
            Return New Ray(intersect, r1.myVector.negative)
        End If

        Dim x, y, z, u, v, w As Double
        x = o1.myVector.myX
        y = o1.myVector.myY
        z = o1.myVector.myZ

        u = norm.myX
        v = norm.myY
        w = norm.myZ

        Dim outR, outQ, outP As Double
        outP = (u * (u * x + v * y + z * w) * (1 - Math.Cos(-angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * x * Math.Cos(-angleIncid) +
              Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-w * y + v * z) * Math.Sin(-angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outQ = (v * (u * x + v * y + z * w) * (1 - Math.Cos(-angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * y * Math.Cos(-angleIncid) +
            Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (w * x - u * z) * Math.Sin(-angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outR = (w * (u * x + v * y + z * w) * (1 - Math.Cos(-angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * z * Math.Cos(-angleIncid) +
            Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-v * x + u * y) * Math.Sin(-angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outRay = New Ray(intersect, New Vector(-outP, -outQ, -outR))
        If Vector.isParallel(outRay.myVector, r1.myVector) Then
            outP = (u * (u * x + v * y + z * w) * (1 - Math.Cos(angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * x * Math.Cos(angleIncid) +
           Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-w * y + v * z) * Math.Sin(angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outQ = (v * (u * x + v * y + z * w) * (1 - Math.Cos(angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * y * Math.Cos(angleIncid) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (w * x - u * z) * Math.Sin(angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outR = (w * (u * x + v * y + z * w) * (1 - Math.Cos(angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * z * Math.Cos(angleIncid) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-v * x + u * y) * Math.Sin(angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outRay = New Ray(intersect, New Vector(-outP, -outQ, -outR))
        End If

        Return outRay
    End Function

    Private Sub numSample_ValueChanged(sender As Object, e As EventArgs) Handles numSample.ValueChanged
        Dim previous = aptArr.Length
        ReDim Preserve aptArr(numSample.Value), phiArr(numSample.Value), coreRadArr(numSample.Value), thetaArr(numSample.Value)
        samples = numSample.Value
        If previous > samples Then GoTo finish : 
        Dim rng = New Random
        Dim rand = New Random
        For i = previous - 1 To samples
            aptArr(i) = CDbl((rand.Next(apeture * 1000) + 1) / 1000) 'aperture *1000 choices
            If rng.Next(0, 2) = 0 Then
                aptArr(i) *= -1
            End If
            phiArr(i) = rand.Next(361) / 180 * Math.PI ' 360 choices
            coreRadArr(i) = CDbl((rand.Next(coreRad * 10000 + 1) / 10000)) ' coreRad * 10 choices
            thetaArr(i) = CDbl(rand.Next(361) / 180 * Math.PI) ' 360 choices
        Next
finish:
    End Sub

    Private Sub SaveDataToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveDataToolStripMenuItem.Click
        StatusNotif.Text = "Saving..."
        Update()
        ' Set a variable to the My Documents path.
        Dim mydocpath As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Dim fileDateTime As String = DateTime.Now.ToString("yyyyMMdd") & "_" & DateTime.Now.ToString("HHmmss")

        Dim filename As String

        If (btnLength.Checked) Then
            filename = "Jitter_sim_" + fileDateTime + ".txt"
            ' Write the string array to a new file named "WriteLines.txt".
            Using outputFile As New StreamWriter(mydocpath & Convert.ToString("\" + filename))
                For i = 0 To samples - 1
                    outputFile.WriteLine(CStr(lengths(0, i)) + vbTab + CStr(lengths(1, i)) + vbTab + CStr(lengths(2, i)) + vbTab + CStr(lengths(3, i)))
                Next
            End Using
        Else
            filename = "Intensity_sim_" + fileDateTime + ".txt"
            Using outputFile As New StreamWriter(mydocpath & Convert.ToString("\" + filename))
                outputFile.WriteLine(ctValid.Text + vbTab + ctInval.Text + vbTab + ctValidT.Text + vbTab + ctInvalT.Text)
                For i = 1 To bxSampPop.Value - 1
                    StatusNotif.Text = "Simulation #" & i
                    Update()

                    UpdateForm()
                    outputFile.WriteLine(ctValid.Text + vbTab + ctInval.Text + vbTab + ctValidT.Text + vbTab + ctInvalT.Text)
                Next
            End Using
        End If

        StatusNotif.Text = "Saved as " + filename + "."
    End Sub

    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        StatusNotif.Text = "Working..."
        Me.Update()

        UpdateForm()

        StatusNotif.Text = "Complete..."
    End Sub


    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Application.ExitThread()
    End Sub

    Private Sub AutoUpdate_Click(sender As Object, e As EventArgs) Handles mnuAutoUpdate.Click
        If mnuAutoUpdate.Checked Then
            Dim ctrl As Object
            For Each ctrl In GroupBox1.Controls
                If TypeOf ctrl Is NumericUpDown Then
                    Dim ctl As NumericUpDown = ctrl

                    AddHandler ctl.ValueChanged, AddressOf param_ValueChanged
                End If
            Next
        Else
            Dim ctrl As Object
            For Each ctrl In GroupBox1.Controls
                If TypeOf ctrl Is NumericUpDown Then
                    Dim ctl As NumericUpDown = ctrl

                    RemoveHandler ctl.ValueChanged, AddressOf param_ValueChanged
                End If
            Next
        End If
    End Sub

    Private Sub mnuAutoScale_Click(sender As Object, e As EventArgs) Handles mnuAutoScale.Click
        If mnuAutoScale.Checked Then
            With EmiDist.ChartAreas(0)
                .AxisX.Minimum = Double.NaN
                .AxisX.Maximum = Double.NaN
                .AxisY.Minimum = Double.NaN
                .AxisY.Maximum = Double.NaN
            End With
            With RefDist.ChartAreas(0)
                .AxisX.Minimum = Double.NaN
                .AxisX.Maximum = Double.NaN
                .AxisY.Minimum = Double.NaN
                .AxisY.Maximum = Double.NaN
            End With

            With TranDist.ChartAreas(0)
                .AxisX.Minimum = Double.NaN
                .AxisX.Maximum = Double.NaN
                .AxisY.Minimum = Double.NaN
                .AxisY.Maximum = Double.NaN
            End With
        Else

            With EmiDist.ChartAreas(0)
                .AxisX.Minimum = origin.x - coreRad * factor
                .AxisX.Maximum = origin.x + coreRad * factor
                .AxisY.Minimum = origin.y - coreRad * factor
                .AxisY.Maximum = origin.y + coreRad * factor
            End With
            With RefDist.ChartAreas(0)
                .AxisX.Minimum = centerIntersect1.x - coreRad * factor
                .AxisX.Maximum = centerIntersect1.x + coreRad * factor
                .AxisY.Minimum = centerIntersect1.y - coreRad * factor
                .AxisY.Maximum = centerIntersect1.y + coreRad * factor
            End With

            With TranDist.ChartAreas(0)
                .AxisX.Minimum = centerIntersect2.x - coreRad * factor
                .AxisX.Maximum = centerIntersect2.x + coreRad * factor
                .AxisY.Minimum = centerIntersect2.y - coreRad * factor
                .AxisY.Maximum = centerIntersect2.y + coreRad * factor
            End With
        End If




    End Sub

    Private Sub ScalingToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ScalingToolStripMenuItem.Click
retry:
        Dim ret = InputBox("Please enter the scaling ratio.", "Scaling Ratio Input", factor.ToString)
        Try
            factor = CDbl(ret)
            mnuAutoScale_Click(Nothing, Nothing)
        Catch ex As InvalidCastException
            GoTo retry
        End Try
    End Sub

    Private Function refract(r1 As Ray, intersect As Point3D, o1 As Ray, norm As Vector, angleIncid As Double, transAngle As Double) As Ray
        Dim outRay As Ray
        Dim x, y, z, u, v, w As Double
        If (angleIncid = 0) Then
            Return New Ray(intersect, r1.myVector)
        End If
        x = o1.myVector.myX
        y = o1.myVector.myY
        z = o1.myVector.myZ

        u = norm.myX
        v = norm.myY
        w = norm.myZ

        Dim outR, outQ, outP As Double

        outP = (u * (u * x + v * y + z * w) * (1 - Math.Cos(-angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * x * Math.Cos(-angleIncid) +
             Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-w * y + v * z) * Math.Sin(-angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outQ = (v * (u * x + v * y + z * w) * (1 - Math.Cos(-angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * y * Math.Cos(-angleIncid) +
            Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (w * x - u * z) * Math.Sin(-angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

        outR = (w * (u * x + v * y + z * w) * (1 - Math.Cos(-angleIncid)) + (u ^ 2 + w ^ 2 + v ^ 2) * z * Math.Cos(-angleIncid) +
            Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-v * x + u * y) * Math.Sin(-angleIncid)) / (u ^ 2 + v ^ 2 + w ^ 2)

        Dim vect = New Vector(outP, outQ, outR)
        Dim angleChange = Math.PI - transAngle
        If (Vector.isParallel(vect, r1.myVector)) Then
            outP = (u * (u * x + v * y + z * w) * (1 - Math.Cos(angleChange)) + (u ^ 2 + w ^ 2 + v ^ 2) * x * Math.Cos(angleChange) +
            Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-w * y + v * z) * Math.Sin(angleChange)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outQ = (v * (u * x + v * y + z * w) * (1 - Math.Cos(angleChange)) + (u ^ 2 + w ^ 2 + v ^ 2) * y * Math.Cos(angleChange) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (w * x - u * z) * Math.Sin(angleChange)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outR = (w * (u * x + v * y + z * w) * (1 - Math.Cos(angleChange)) + (u ^ 2 + w ^ 2 + v ^ 2) * z * Math.Cos(angleChange) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-v * x + u * y) * Math.Sin(angleChange)) / (u ^ 2 + v ^ 2 + w ^ 2)


        Else
            outP = (u * (u * x + v * y + z * w) * (1 - Math.Cos(-angleChange)) + (u ^ 2 + w ^ 2 + v ^ 2) * x * Math.Cos(-angleChange) +
            Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-w * y + v * z) * Math.Sin(-angleChange)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outQ = (v * (u * x + v * y + z * w) * (1 - Math.Cos(-angleChange)) + (u ^ 2 + w ^ 2 + v ^ 2) * y * Math.Cos(-angleChange) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (w * x - u * z) * Math.Sin(-angleChange)) / (u ^ 2 + v ^ 2 + w ^ 2)

            outR = (w * (u * x + v * y + z * w) * (1 - Math.Cos(-angleChange)) + (u ^ 2 + w ^ 2 + v ^ 2) * z * Math.Cos(-angleChange) +
                Math.Sqrt(u ^ 2 + w ^ 2 + v ^ 2) * (-v * x + u * y) * Math.Sin(-angleChange)) / (u ^ 2 + v ^ 2 + w ^ 2)

        End If
        outRay = New Ray(intersect, New Vector(-outP, -outQ, -outR))
        Return outRay
    End Function

    Private Sub resetSeriesPt()
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is DataVisualization.Charting.Chart Then
                Dim ctr As DataVisualization.Charting.Chart = ctrl
                With ctr
                    For Each arr As DataVisualization.Charting.Series In .Series
                        arr.Points.Clear()
                    Next
                End With
            End If
        Next

    End Sub


    Private Sub histogram2(arr As DataVisualization.Charting.DataPoint(),
                           targ As DataVisualization.Charting.Chart)
        If arr.Length < 2 Then
            Exit Sub
        End If

        Dim chtWid = targ.Height

        Dim numBin = bxBinCt.Value 'Input here
        Dim binWid = chtWid / numBin
        If (targ.Series.IndexOf("Histogram") > -1) Then GoTo skip
        targ.Series.Add("Histogram")
skip:
        targ.Series("Histogram").Points.Clear()
        targ.Series("Histogram").ChartType = DataVisualization.Charting.SeriesChartType.Bar
        targ.Series("Histogram").MarkerSize = binWid
        targ.ChartAreas(0).AxisX.Title = "Optical Length (mm)"


        Dim statArr(arr.Length - 1), statArrX(arr.Length - 1) As Decimal

        For i = 0 To arr.Length - 1
            statArr(i) = arr(i).YValues(0)
        Next
        Array.Sort(statArr)


        Dim dataDistrib(numBin) As Integer
        Dim rangeY = statArr(statArr.Length - 1) - statArr(0)
        Dim incrementY = rangeY / (numBin - 1)

        Dim low, high, current As Decimal
        low = statArr(0) + incrementY * (current - 1)
        current = 1
        high = statArr(0) + incrementY * current
        For i = 0 To statArr.Length - 1
            While statArr(i) > high
                current += 1
                high = statArr(0) + incrementY * current
                low = statArr(0) + incrementY * (current - 1)
            End While
            If statArr(i) >= low And statArr(i) <= high Then
                dataDistrib(current - 1) += 1
            End If

        Next
        'Chart Area width / num bins = point.width
        'Multiply point by 10 until diff between points > 1, count
        'Group scaled points into categories, divide back.
        'Input # of bins
        For i = 0 To numBin - 1
            targ.Series("Histogram").Points.AddXY((statArr(0) + incrementY * i), dataDistrib(i))
            targ.Series("Histogram").Points.Item(i).ToolTip = targ.Series("Histogram").Points.Item(i).ToString

        Next
    End Sub
    Private Sub histogram(arr As DataVisualization.Charting.DataPoint(),
                           targ As DataVisualization.Charting.Chart,
                           stdev As Decimal, avg As Decimal)

        If arr.Length < 2 Then
            Exit Sub
        End If
        Dim chtWid = targ.Height

        Dim numBin = bxBinCt.Value 'Input here
        Dim binWid = chtWid / numBin
        If (targ.Series.IndexOf("Histogram") > -1) Then GoTo skip
        targ.Series.Add("Histogram")
skip:
        targ.Series("Histogram").Points.Clear()
        targ.Series("Histogram").ChartType = DataVisualization.Charting.SeriesChartType.Bar
        targ.Series("Histogram").MarkerSize = binWid
        targ.ChartAreas(0).AxisX.Title = "StDev"


        Dim statArr(arr.Length - 1), statArrX(arr.Length - 1) As Decimal

        For i = 0 To arr.Length - 1
            statArr(i) = arr(i).YValues(0)
        Next
        Array.Sort(statArr)

        Dim dataDistrib(numBin) As Integer


        Dim base, max, low, high, current As Decimal
        Dim stDevBase As Integer = (Math.Truncate((statArr(0) - avg) / stdev) - 1)
        base = avg + stdev * (Math.Truncate((statArr(0) - avg) / stdev) - 1)
        max = avg + stdev * (Math.Truncate((statArr(statArr.Length - 1) - avg) / stdev) + 1)

        Dim rangeY As Integer = (max - base) / stdev ' number of stdevs to parse
        Dim stDevY = rangeY / numBin
        Dim incrementY = stDevY * stdev
        current = 1
        low = base
        high = base + incrementY
        For i = 0 To statArr.Length - 1
            While statArr(i) > high
                current += 1
                high = base + (current) * incrementY
                low = base + (current - 1) * incrementY
            End While
            If statArr(i) >= low And statArr(i) <= high Then
                dataDistrib(current - 1) += 1
            End If

        Next
        'Chart Area width / num bins = point.width
        'Multiply point by 10 until diff between points > 1, count
        'Group scaled points into categories, divide back.
        'Input # of bins
        For i = 0 To numBin - 1
            targ.Series("Histogram").Points.AddXY(stDevY * i + stDevBase, dataDistrib(i))
            targ.Series("Histogram").Points.Item(i).ToolTip = targ.Series("Histogram").Points.Item(i).ToString
        Next

    End Sub


    Private Sub chgGraphStyle_CheckedChanged(sender As Object, e As EventArgs) Handles StDevHist.CheckedChanged
        If validCt1 = 0 Or validCt2 = 0 Then
            Exit Sub
        End If
        If StDevHist.Checked Then
            '.Series("Normal").Enabled = OutToRef.Series("Central").Enabled = OutToRef.Series("Median").Enabled = OutToRef.Series("Average").Enabled = False
            'OutToTran.Series("Normal").Enabled = OutToTran.Series("Central").Enabled = OutToTran.Series("Median").Enabled = OutToTran.Series("Average").Enabled = False

            Dim stdev = findstDev(OutToRef.Series(0).Points.ToArray(), ctrlL, OutToRef)
            histogram(OutToRef.Series("Normal").Points.ToArray, OutToRef, stdev, total1 / validCt1)
            stdev = findstDev(OutToTran.Series(0).Points.ToArray(), ctrlL2, OutToTran)
            histogram(OutToTran.Series("Normal").Points.ToArray, OutToTran, stdev, total2 / validCt2)
        Else
            'OutToRef.Series("Normal").Enabled = OutToRef.Series("Central").Enabled = OutToRef.Series("Median").Enabled = OutToRef.Series("Average").Enabled = True
            'OutToTran.Series("Normal").Enabled = OutToTran.Series("Central").Enabled = OutToTran.Series("Median").Enabled = OutToTran.Series("Average").Enabled = True

            histogram2(OutToRef.Series("Normal").Points.ToArray, OutToRef)
            histogram2(OutToTran.Series("Normal").Points.ToArray, OutToTran)
        End If

    End Sub



    'When this method is called, the program will calculate and compare the passed ray
    'with the central ray and determine whether it goes in the fiber
    'Variables for Central Ray: centerTheta1, centerTheta2, centerRay1, centerRay2
    'centerTheta1 = findAngle2(r2, orthVect)
    ''reference for theta inside the fiber
    '    centerTheta2 = snellLaw(centerTheta1, 1, 1.5)

    'Dim intersect = Point3D.addVector(r2.myVector, r2.myOrigin)
    '    centerRay1 = r2
    '    centerRay2 = refract(r2, intersect, New Ray(intersect, orthVect), plnVect, centerTheta1, centerTheta2)

    Private Function refractFiber(r1 As Ray, intersect As Point3D, centerRay As Ray, centerIntersect As Point3D, n1 As Double, n2 As Double) As Boolean

        'Outside angle to adjust fiber 
        Dim outsideTheta = findAngle2(r1, orthVect)
        'reference for theta inside the fiber
        Dim fiberTheta = snellLaw(outsideTheta, n1, n2)

        Dim refCenter As Ray = refract(r1, intersect, New Ray(intersect, orthVect), plnVect, outsideTheta, fiberTheta)
        'angle between Ray and Center Ray
        Dim betAngle = findAngle(centerRay, refCenter)

        If Vector.pt2pt(intersect, centerIntersect).getLength > (bxCoreRad.Value / 1000) Then
            counterOoR += 1
            'Console.WriteLine("Out of range")
            Return False
        End If
        If betAngle < innerNA Then

            Return True

        Else

            'Console.WriteLine(False)
            counterFalse += 1
            Return False

        End If

    End Function

    Private Function refractFiberRet(r1 As Ray, intersect As Point3D, centerRay As Ray, centerIntersect As Point3D, n1 As Double, n2 As Double) As Ray

        'Outside angle to adjust fiber 
        Dim outsideTheta = findAngle2(r1, orthVect)
        'reference for theta inside the fiber
        Dim fiberTheta = snellLaw(outsideTheta, 1, 1.5)

        Dim refCenter As Ray = refract(r1, intersect, New Ray(intersect, orthVect), plnVect, outsideTheta, fiberTheta)
        'angle between Ray and Center Ray
        Dim betAngle = findAngle(centerRay, refCenter)


        Return refCenter
    End Function
    Private Function findAngle(r1 As Ray, r2 As Ray) As Double
        Return Math.Acos(Vector.dotProduct(r1.myVector, r2.myVector) / (r1.myVector.getLength * r2.myVector.getLength))
    End Function

    Private Function findAngle2(r1 As Ray, r2 As Vector) As Double
        Return Math.Acos(Vector.dotProduct(r1.myVector, r2) / (r1.myVector.getLength * r2.getLength))
    End Function

    Private Function snellLaw(a1 As Double, n1 As Double, n2 As Double)
        Return Math.Asin(n1 / n2 * Math.Sin(a1))
    End Function

    Private Sub resetTxtBx()

        refAb(0) = 0
        refAb(1) = 0
        refBe(0) = 0
        refMax(0) = 0
        refBe(1) = 0
        refMax(1) = 0
        refMin(0) = 10000
        refMin(1) = 10000
        refMax(0) = 0
        refMax(1) = 0
        counterFalse = 0
        counterOoR = 0
    End Sub
End Class



''Working Histogram code

'Private Sub histogram(arr As System.Windows.Forms.DataVisualization.Charting.DataPoint(),
'                       targ As System.Windows.Forms.DataVisualization.Charting.Chart, stdev As Double)
'    Dim chtWid = targ.Height

'    Dim numBin = bxBinCt.Value 'Input here
'    Dim binWid = chtWid / numBin
'    If (targ.Series.IndexOf("Histogram") > -1) Then GoTo skip
'    targ.Series.Add("Histogram")
'skip:
'    targ.Series("Histogram").Points.Clear()
'    targ.Series("Histogram").ChartType = DataVisualization.Charting.SeriesChartType.Bar
'    targ.Series("Histogram").MarkerSize = binWid

'    Dim statArr(arr.Length - 1), statArrX(arr.Length - 1) As Double

'    For i = 0 To arr.Length - 1
'        statArr(i) = arr(i).YValues(0)
'    Next
'    Array.Sort(statArr)

'    Dim counter = 0
'    While statArr(1) - statArr(0) < 1
'        For i = 0 To statArr.Length - 1
'            statArr(i) = statArr(i) * 10
'        Next
'        counter += 1
'    End While

'    Dim dataDistrib(numBin) As Integer

'    Dim rangeY = statArr(statArr.Length - 1) - statArr(0)
'    Dim incrementY = rangeY / (numBin - 1)

'    Dim low, high, current As Double
'    low = statArr(0) + incrementY * (current - 1)
'    current = 0
'    high = statArr(0) - (9 - current) * stdev
'    For i = 0 To statArr.Length - 1
'        While statArr(i) > high
'            current += 1
'            high = statArr(0) + incrementY * current
'            low = statArr(0) + incrementY * (current - 1)
'        End While
'        If statArr(i) >= low And statArr(i) <= high Then
'            dataDistrib(current - 1) += 1
'        End If

'    Next
'    'Chart Area width / num bins = point.width
'    'Multiply point by 10 until diff between points > 1, count
'    'Group scaled points into categories, divide back.
'    'Input # of bins
'    For i = 0 To numBin - 1
'        targ.Series("Histogram").Points.AddXY((statArr(0) + incrementY * i) / 10 ^ counter, dataDistrib(i))
'    Next
'End Sub