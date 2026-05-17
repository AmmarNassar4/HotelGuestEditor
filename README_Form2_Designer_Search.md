# Form2 Identity Search with Designer

## المطلوب بعد فك الضغط

انسخ الملفات داخل جذر الريبو بحيث تكون المسارات:

- `HotelGuestEditor/Form2.cs`
- `HotelGuestEditor/Form2.Designer.cs`
- `HotelGuestEditor/IdentityGuestSearchResult.cs`
- `apply_form2_designer_patch.ps1`

ثم شغّل:

```powershell
.\apply_form2_designer_patch.ps1
Remove-Item -Recurse -Force ".\HotelGuestEditor\bin",".\HotelGuestEditor\obj"
dotnet build ".\HotelGuestEditor\HotelGuestEditor.csproj"
```

## ما الذي يفعله السكربت؟

- يضيف زر `btnIdentitySearch` داخل `Form1.Designer.cs`.
- الزر يكون `Visible = false`.
- يضيف الزر ضمن `_editorControls` في `Form1.cs`، لذلك لا يظهر إلا بعد تحميل/اختيار Guest.
- يضيف حدث `btnIdentitySearch_Click`.
- يفتح `Form2` كـ popup tool window.
- قبل تحميل `Form2` يظهر popup لإدخال رقم الهوية/الجواز.
- يبحث في `PMS.GSPASTBL.PASNUB`.
- يرتب النتائج حسب `A.GSTCOD DESC`.
- عند double click يرجع البيانات إلى `Form1`.
- لو يوجد تاريخ ميلاد، يضبط:
  - `chkBirthDate.Checked = true`
  - `dtBirthDate.Value = تاريخ الميلاد`
- يضيف حماية من خطأ `HotelGuestEditor.Form1.resources`.
