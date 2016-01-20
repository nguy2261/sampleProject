Public Class PriceTrac
    
    'Declare a structure for each product
    'This could be a class instead of structure but for the sake of simplicity and demonstration, I implemented this as a structure
    Structure ProductAttribute
        Dim ID As Integer
        Dim Name As String
        Dim Competitor As String
        Dim Count As Integer
        Dim Price As Double
        Dim Att As String
        Dim Color As String
        Dim Brand As String
        Dim Material As String
        Dim Type As String
        Dim Occasion As String
        Dim Size As String
        Dim SleeveLength As String
        Dim Neckline As String
    End Structure

    Structure Virtual
        Dim att As String
        Dim type As String
    End Structure
    
    'Close the form'
    Private Sub btnQuit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnQuit.Click
        Me.Close()
    End Sub


'Load the database
'In real business settings, this would establish the connection with actual dataware house of the company and get the information ready to be analyzed

    Private Sub PriceTrac_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'TODO: This line of code loads data into the 'PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE' table. You can move, or remove it, as needed.
        Me.COMPETING_PRODUCT_ATTRIBUTETableAdapter.Fill(Me.PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE)
        'TODO: This line of code loads data into the 'PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT' table. You can move, or remove it, as needed.
        Me.COMPETING_PRODUCTTableAdapter.Fill(Me.PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT)
        'TODO: This line of code loads data into the 'PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE' table. You can move, or remove it, as needed.
        Me.PRODUCT_ATTRIBUTETableAdapter.Fill(Me.PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE)
        'TODO: This line of code loads data into the 'PrototypeDatabase_v2_KushDataSet.ATTRIBUTE' table. You can move, or remove it, as needed.
        Me.ATTRIBUTETableAdapter.Fill(Me.PrototypeDatabase_v2_KushDataSet.ATTRIBUTE)
        'TODO: This line of code loads data into the 'PrototypeDatabase_v2_KushDataSet.PRODUCT' table. You can move, or remove it, as needed.
        Me.PRODUCTTableAdapter.Fill(Me.PrototypeDatabase_v2_KushDataSet.PRODUCT)
    End Sub


'UI Desisn and Implementation for each button
    Private Sub btnAtt_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAtt.Click
        GroupBox.Visible = True
    End Sub

'Start of UI Design
    Private Sub chkColor_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkColor.CheckedChanged
        If chkColor.Checked Then
            txtColor.Visible = True
        Else
            txtColor.Visible = False
        End If
    End Sub

    Private Sub chkMaterial_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMaterial.CheckedChanged
        If chkMaterial.Checked Then
            txtMaterial.Visible = True
        Else
            txtMaterial.Visible = False
        End If
    End Sub

    Private Sub chkBrand_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkBrand.CheckedChanged
        If chkBrand.Checked Then
            txtBrand.Visible = True
        Else
            txtBrand.Visible = False
        End If
    End Sub

    Private Sub chkType_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkType.CheckedChanged
        If chkType.Checked Then
            txtType.Visible = True
        Else
            txtType.Visible = False
        End If
    End Sub

    Private Sub chkOccasion_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkOccasion.CheckedChanged
        If chkOccasion.Checked Then
            txtOccasion.Visible = True
        Else
            txtOccasion.Visible = False
        End If
    End Sub

    Private Sub chkSize_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSize.CheckedChanged
        If chkSize.Checked = True Then
            txtSize.Visible = True
        Else
            txtSize.Visible = False
        End If
    End Sub

    Private Sub chkSizeLength_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSizeLength.CheckedChanged
        If chkSizeLength.Checked Then
            txtSizeLength.Visible = True
        Else
            txtSizeLength.Visible = False
        End If
    End Sub

    Private Sub chkNeck_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNeck.CheckedChanged
        If chkNeck.Checked Then
            txtNeckline.Visible = True
        Else
            txtNeckline.Visible = False
        End If
    End Sub

'End of UI Design

    'Display the product button
    Private Sub btnDisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDisplay.Click
        Dim input As String = InputBox("Please enter the SKU", "SKU")
        If IsNumeric(input) Then 'Check input
        
            'Find the product name and price by SKU in the PRODUCT Table
            Dim QueryName = From product In PrototypeDatabase_v2_KushDataSet.PRODUCT
                            Where product.PRO_SKU = CInt(input)
                            Select product.PRO_Name, product.PRO_Price

            If QueryName.Count > 0 Then
                'Find the product attribute by SKU in Attributes table
                Dim QueryProduct = From Pro In PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE
                                   Order By Pro.PRO_SKU
                                   Where Pro.PRO_SKU = input
                                   Select Pro.PRO_SKU, Pro.ATT_Name, Pro.ATT_Type


                'Display the product in the listbox
                lstProduct.Items.Add("Name: " & QueryName.First.PRO_Name)
                lstProduct.Items.Add("Price: " & FormatCurrency(QueryName.First.PRO_Price))
                lstProduct.Items.Add("Attributes")

                For i As Integer = 0 To QueryProduct.Count - 1
                    lstProduct.Items.Add("- " & QueryProduct(i).ATT_Name)
                Next
            Else
                MessageBox.Show("You entered an invalid SKU", "Error")
            End If
        Else
            MessageBox.Show("You entered an invalid SKU", "Error")
        End If
    End Sub


    'Button Click
    'Main button to get the attributes and search for product'
    Private Sub btnSKU_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSKU.Click
        lstProduct.Items.Clear()
        Dim input As String = mskSKU.Text
        If IsNumeric(input) Then 'Check the SKU whether it exists in the database or not
            Dim QueryName = From product In PrototypeDatabase_v2_KushDataSet.PRODUCT
                            Where (product.PRO_SKU = CInt(input))
                            Select product.PRO_Name

            If QueryName.Count > 0 Then
                'Find the attribute of our products
                Dim QueryProduct = From Pro In PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE
                                                   Order By Pro.PRO_SKU
                                                   Where Pro.PRO_SKU = input
                                                   Select Pro.ATT_Name

                Dim Attribute(QueryProduct.Count - 1) As String
                For i As Integer = 0 To Attribute.Count - 1
                    Attribute(i) = QueryProduct(i)
                Next

                'Find the attribute and match with the our SKU product
                Dim QuerySearch = From Comp_Pro In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE
                                  Join Pro In PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE
                                  On Comp_Pro.ATT_Name Equals Pro.ATT_Name
                                  Join CompetingProduct In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT
                                  On Comp_Pro.CPRO_ID Equals CompetingProduct.CPRO_ID
                                  Where Pro.PRO_SKU = input
                                  Order By Comp_Pro.CPRO_ID Ascending
                                  Select Comp_Pro.CPRO_ID, CompetingProduct.CMP_Name, CompetingProduct.CPRO_Name, CompetingProduct.CPRO_Price

                'Count how many match items that matched the search 
                Dim Shorten = From CPro In QuerySearch
                               Select CPro.CPRO_ID, CPro.CMP_Name, CPro.CPRO_Name, CPro.CPRO_Price Distinct

                Dim Product(Shorten.Count - 1) As ProductAttribute
                For i As Integer = 0 To Shorten.Count - 1
                    For j As Integer = 0 To QuerySearch.Count - 1
                        If Shorten(i).CPRO_ID = QuerySearch(j).CPRO_ID Then
                            Product(i).Count += 1
                        End If
                    Next
                Next


                For i As Integer = 0 To Shorten.Count - 1
                    Product(i).Name = Shorten(i).CPRO_Name
                    Product(i).ID = Shorten(i).CPRO_ID
                    Product(i).Competitor = Shorten(i).CMP_Name
                    Product(i).Price = Shorten(i).CPRO_Price
                Next


                For i As Integer = 0 To Shorten.Count - 1
                    Find(input, Shorten(i).CPRO_ID, Product, i)
                    FindAtt(Attribute, Product, Shorten(i).CPRO_ID, i)
                Next

                Dim minMat As Integer = 0
                If IsNumeric(mskMin.Text) Then
                    If mskMin.Text > 0 And mskMin.Text < 9 Then
                        minMat = mskMin.Text
                    End If
                Else
                    MessageBox.Show("You entered an invalid minimum matched attributes. The system automatically set 0 for this searching", "Error")
                End If

                Dim Query = From sth In Product
                            Let Weight = sth.Count
                            Let Price = FormatCurrency(sth.Price)
                            Where Weight >= minMat
                            Order By Weight Descending
                            Select Weight, sth.ID, sth.Name, sth.Competitor, Price, sth.Color, sth.Brand, sth.Material, sth.Type, sth.Occasion, sth.Size, sth.SleeveLength, sth.Neckline

                dgvOutput.DataSource = Query.ToList
                dgvOutput.Columns("Weight").HeaderText = "Matched Count"

                Dim SugPrice As Double = 0
                Dim totalweight As Integer = 0
                For i As Integer = 0 To Query.Count - 1
                    SugPrice += Query(i).Price * Query(i).Weight
                    totalweight += Query(i).Weight
                Next
                lstProduct.Items.Add("The suggested price for this product is " & FormatCurrency(SugPrice / totalweight))
            Else
                MessageBox.Show("You did not enter the SKU or enter an invalid SKU", "Error")
            End If
        Else
            MessageBox.Show("You entered an invalid SKU", "Error")
        End If
    End Sub

    Sub Find(ByVal SKU As Integer, ByVal ID As Integer, ByRef Product() As ProductAttribute, ByVal Index As Integer)
        Dim Query = From Comp_Pro In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE
                                  Join Pro In PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE
                                  On Comp_Pro.ATT_Name Equals Pro.ATT_Name
                                  Join CompetingProduct In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT
                                  On Comp_Pro.CPRO_ID Equals CompetingProduct.CPRO_ID
                                  Where Comp_Pro.CPRO_ID = ID And Pro.PRO_SKU = SKU
                                  Select Comp_Pro.ATT_Name

        Dim result As String = ""
        For i As Integer = 0 To Query.Count - 1
            result &= Query(i) & ", "
        Next
        Product(Index).Att = result
    End Sub


    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearch.Click
        lstProduct.Items.Clear()
        Dim att_color() As String = Nothing
        Dim att_brand() As String = Nothing
        Dim att_material() As String = Nothing
        Dim att_type() As String = Nothing
        Dim att_occasion() As String = Nothing
        Dim att_size() As String = Nothing
        Dim att_sizelength() As String = Nothing
        Dim att_neckline() As String = Nothing
        Dim attribute(7) As String

        'Modify the button
        Dim CombineString As String = ""
        If chkColor.Checked Then
            SearchAtt(att_color, txtColor.Text)
            attribute(0) = txtColor.Text
        End If

        If chkBrand.Checked Then
            SearchAtt(att_brand, txtBrand.Text)
            attribute(1) = txtBrand.Text
        End If

        If chkMaterial.Checked Then
            SearchAtt(att_material, txtMaterial.Text)
            attribute(2) = txtMaterial.Text
        End If

        If chkType.Checked Then
            SearchAtt(att_type, txtType.Text)
            attribute(3) = txtType.Text
        End If

        If chkOccasion.Checked Then
            SearchAtt(att_occasion, txtOccasion.Text)
            attribute(4) = txtOccasion.Text
        End If

        If chkSize.Checked Then
            SearchAtt(att_size, txtSize.Text)
            attribute(5) = txtSize.Text
        End If

        If chkSizeLength.Checked Then
            SearchAtt(att_sizelength, txtSizeLength.Text)
            attribute(6) = txtSizeLength.Text
        End If

        If chkNeck.Checked Then
            SearchAtt(att_neckline, txtNeckline.Text)
            attribute(7) = txtNeckline.Text
        End If

        Dim StringATT As String = ""
        StringATT = txtColor.Text & "," & txtBrand.Text & "," & txtMaterial.Text & "," & txtNeckline.Text & ","
        StringATT &= txtOccasion.Text & "," & txtSize.Text & "," & txtSizeLength.Text & "," & txtType.Text

        Dim ColorBrand() As String = JoinArray(att_color, att_brand)
        Dim MaterialType() As String = JoinArray(att_material, att_neckline)
        Dim OccSize() As String = JoinArray(att_occasion, att_size)
        Dim TypeLength() As String = JoinArray(att_type, att_sizelength)
        Dim CM() As String = JoinArray(ColorBrand, MaterialType)
        Dim OT() As String = JoinArray(OccSize, TypeLength)
        Dim Att_total() As String = JoinArray(CM, OT)

        If IsNothing(Att_total) = False Then
            Dim Shorten = From ID In Att_total
                        Select ID Distinct


            Dim Product(Shorten.Count - 1) As ProductAttribute
            For i As Integer = 0 To Shorten.Count - 1
                For j As Integer = 0 To Att_total.Count - 1
                    If Shorten(i) = Att_total(j) Then
                        Product(i).Count += 1
                    End If
                Next
            Next

            For i As Integer = 0 To Product.Count - 1
                FindCompeting(attribute, Product, Shorten(i), i)
                FindAtt(attribute, Product, Shorten(i), i)
            Next


            Dim minMat As Integer = 0
            If IsNumeric(mskMinAtt.Text) Then
                If mskMinAtt.Text > 0 And mskMinAtt.Text < 9 Then
                    minMat = mskMinAtt.Text
                End If
            Else
                MessageBox.Show("You entered an invalid minimum matched attributes. The system automatically set 0 for this searching", "Error")
            End If


            Dim Query = From sth In Product
                        Let Weight = sth.Count
                        Let Price = FormatCurrency(sth.Price)
                        Where Weight >= minMat
                        Order By Weight Descending
                        Select Weight, sth.ID, sth.Name, sth.Competitor, Price, sth.Color, sth.Brand, sth.Material, sth.Type, sth.Occasion, sth.Size, sth.SleeveLength, sth.Neckline

            dgvOutput.DataSource = Query.ToList
            dgvOutput.Columns("Weight").HeaderText = "Matched Count"

            Dim SugPrice As Double = 0
            Dim totalweight As Integer = 0
            For i As Integer = 0 To Query.Count - 1
                SugPrice += Query(i).Price * Query(i).Weight
                totalweight += Query(i).Weight
            Next
            lstProduct.Items.Add("The suggested price for this product is " & FormatCurrency(SugPrice / totalweight))
        Else
            MessageBox.Show("The system cannot find any matching result", "Error")
        End If
    End Sub


    Sub SearchAtt(ByRef att_arr() As String, ByVal att_name As String)
        Dim QuerySearchAtt = From att In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE
                             Where att.ATT_Name = att_name
                             Let ID = att.CPRO_ID
                             Order By ID Ascending
                             Select ID Distinct

        If QuerySearchAtt.Count > 0 Then
            ReDim att_arr(QuerySearchAtt.Count - 1)
            For i As Integer = 0 To att_arr.Count - 1
                att_arr(i) = QuerySearchAtt(i)
            Next
            'Else
            'lstProduct.Items.Add(att_name & " is not in the array")
            'ReDim att_arr(0)
            'att_arr(0) = ""
        End If
    End Sub

    Function JoinArray(ByVal arr1() As String, ByVal arr2() As String) As String()
        Dim arr3() As String
        If IsNothing(arr1) And IsNothing(arr2) Then
            Return Nothing
        ElseIf IsNothing(arr1) And IsNothing(arr2) = False Then
            Return arr2
        ElseIf IsNothing(arr2) And IsNothing(arr1) = False Then
            Return arr1
        Else
            ReDim arr3(arr1.Count + arr2.Count - 1)
            For i As Integer = 0 To arr1.Count - 1
                arr3(i) = arr1(i)
            Next
            For i As Integer = 0 To arr2.Count - 1
                arr3(arr1.Count - 1 + i + 1) = arr2(i)
            Next
            Return arr3
        End If
    End Function

    Sub FindCompeting(ByVal attributeArray() As String, ByRef Product() As ProductAttribute, ByVal ID As String, ByVal Index As Integer)
        Dim Query1 = From Pro In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT
                    Where Pro.CPRO_ID = ID
                    Select Pro.CPRO_Name, Pro.CMP_Name, Pro.CPRO_Price, Pro.CPRO_ID

        Product(Index).ID = Query1.First.CPRO_ID
        Product(Index).Name = Query1.First.CPRO_Name
        Product(Index).Competitor = Query1.First.CMP_Name
        Product(Index).Price = Query1.First.CPRO_Price

        Dim Query = From Comp_Pro In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE
                                  Join Pro In PrototypeDatabase_v2_KushDataSet.PRODUCT_ATTRIBUTE
                                  On Comp_Pro.ATT_Name Equals Pro.ATT_Name
                                  Join Att In attributeArray On Comp_Pro.ATT_Name Equals Att
                                  Join CompetingProduct In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT
                                  On Comp_Pro.CPRO_ID Equals CompetingProduct.CPRO_ID
                                  Where (Comp_Pro.CPRO_ID = ID)
                                  Select Comp_Pro.ATT_Name Distinct

        Dim result As String = ""
        For i As Integer = 0 To Query.Count - 1
            result &= Query(i) & ", "
        Next
        Product(Index).Att = result
        

    End Sub

    Sub FindAtt(ByVal attribute() As String, ByRef Product() As ProductAttribute, ByVal ID As String, ByVal index As Integer)
        Dim Query = From Pro In PrototypeDatabase_v2_KushDataSet.COMPETING_PRODUCT_ATTRIBUTE
                    Join att In attribute On Pro.ATT_Name Equals att
                    Join Attri In PrototypeDatabase_v2_KushDataSet.ATTRIBUTE On
                    Pro.ATT_Name Equals Attri.ATT_Name
                    Where Pro.CPRO_ID = ID
                    Select Attri.ATT_Type, Pro.ATT_Name

        For i As Integer = 0 To Query.Count - 1
            If Query(i).ATT_Type = "Color" Then
                Product(index).Color = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Brand" Then
                Product(index).Brand = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Material" Then
                Product(index).Material = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Type" Then
                Product(index).Type = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Occasion" Then
                Product(index).Occasion = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Size" Then
                Product(index).Size = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Sleeve length" Then
                Product(index).SleeveLength = Query(i).ATT_Name
            ElseIf Query(i).ATT_Type = "Neckline" Then
                Product(index).Neckline = Query(i).ATT_Name
            End If
        Next

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        txtColor.Text = "Red"
        txtBrand.Text = "Columbia"
        txtMaterial.Text = "Polyester"
        txtOccasion.Text = "Day"
        txtType.Text = "Jacket"
        txtSizeLength.Text = "Full Sleeve"
    End Sub
End Class
