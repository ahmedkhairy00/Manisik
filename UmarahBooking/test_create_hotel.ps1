# Test CreateHotel Endpoint Script

$baseUrl = "http://localhost:5000" # Adjust port if needed
$adminEmail = "admin@manisik.com"
$adminPassword = "Admin@123456"

# 1. Login as Admin
Write-Host "Logging in as Admin..." -ForegroundColor Cyan
$loginBody = @{
    email = $adminEmail
    password = $adminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/Auth/Login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    Write-Host "Login successful. Token received." -ForegroundColor Green
}
catch {
    Write-Error "Login failed: $_"
    exit
}

# 2. Create Dummy Image
$imagePath = "$PSScriptRoot\test_image.jpg"
if (-not (Test-Path $imagePath)) {
    Set-Content -Path $imagePath -Value "Dummy Image Content"
}

# 3. Create Hotel
Write-Host "Creating Hotel..." -ForegroundColor Cyan
$headers = @{
    Authorization = "Bearer $token"
}

$form = @{
    Name = "PowerShell Test Hotel"
    City = "Madinah"
    Address = "456 Script Ave"
    StarRating = "4"
    DistanceToHaram = "1.2"
    Description = "Created via PowerShell script"
    DescriptionAr = "تم الإنشاء بواسطة سكريبت"
    image = Get-Item -Path $imagePath
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Hotel/CreateHotel" -Method Post -Headers $headers -Form $form
    
    if ($response.success) {
        Write-Host "Hotel created successfully!" -ForegroundColor Green
        Write-Host "Hotel ID: $($response.data.id)"
        Write-Host "Hotel Name: $($response.data.name)"
    }
    else {
        Write-Host "Failed to create hotel: $($response.message)" -ForegroundColor Red
    }
}
catch {
    Write-Error "Request failed: $_"
    Write-Host "Response Body: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

# Cleanup
if (Test-Path $imagePath) {
    Remove-Item $imagePath
}
