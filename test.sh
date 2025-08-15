#!/bin/bash

echo "🚀 Registering 100 test users..."
echo ""

# API endpoint
API_URL="http://localhost:5088/auth/register"

# Counter for successful registrations
success_count=0
error_count=0

for i in {100..200}; do
    # Generate unique username and simple device ID
    username="test_user_${i}"
    device_id="device_${i}"
    
    # Register user
    response=$(curl -s -X POST "$API_URL" \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"password\": \"TestPassword123\",
            \"deviceId\": \"$device_id\"
        }")
    
    # Check if registration was successful
    if echo "$response" | grep -q '"success":true'; then
        echo "✅ User $i: $username registered successfully"
        ((success_count++))
    else
        echo "❌ User $i: $username failed - $(echo "$response" | grep -o '"message":"[^"]*"' | cut -d'"' -f4)"
        ((error_count++))
    fi
    
    # Small delay to avoid overwhelming the API
    sleep 0.1
done

echo ""
echo "📊 Registration Summary:"
echo "   ✅ Successful: $success_count"
echo "   ❌ Failed: $error_count"
echo "   📈 Total: $((success_count + error_count))"
echo ""
echo "🎯 Test completed!"