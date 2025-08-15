#!/bin/bash

echo "🎯 Submitting scores for all users..."
echo ""

# API endpoints
LOGIN_URL="http://localhost:5088/auth/login"
SUBMIT_URL="http://localhost:5088/leaderboard/submit"

# Counters
total_scores=0
success_count=0
error_count=0

# Get current timestamp for replay protection
current_timestamp=$(date +%s)

echo "📊 Starting score submission process..."
echo ""

# Loop through users 1-100
for user_id in {100..200}; do
    username="test_user_${user_id}"
    
    echo "�� Processing user: $username"
    
    # Login to get JWT token
    login_response=$(curl -s -X POST "$LOGIN_URL" \
        -H "Content-Type: application/json" \
        -d "{
            \"username\": \"$username\",
            \"password\": \"TestPassword123\"
        }")
    
    # Extract JWT token
    jwt_token=$(echo "$login_response" | grep -o '"AccessToken":"[^"]*"' | cut -d'"' -f4)
    echo "response: $login_response"
    if [ -z "$jwt_token" ]; then
        echo "❌ Failed to login user: $username"
        ((error_count++))
        continue
    fi
    
    echo "   ✅ Login successful, submitting scores..."
    
    # Submit 50-100 random scores for this user
    scores_count=$((RANDOM % 51 + 50))  # 50-100 arası
    
    for score_index in $(seq 1 $scores_count); do
        # Generate random score between 100-10000
        random_score=$((RANDOM % 9901 + 100))
        
        # Random game mode (Classic or Tournament)
        game_modes=("Classic" "Tournament")
        random_game_mode=${game_modes[$((RANDOM % 2))]}
        
        # Random player level and trophy count
        player_level=$((RANDOM % 50 + 1))
        trophy_count=$((RANDOM % 100))
        
        # Generate unique idempotency key
        idempotency_key="score_${user_id}_${score_index}_$(date +%s)_${RANDOM}"
        
        # Submit score
        submit_response=$(curl -s -X POST "$SUBMIT_URL" \
            -H "Authorization: Bearer $jwt_token" \
            -H "Content-Type: application/json" \
            -H "X-Timestamp: $current_timestamp" \
            -H "Idempotency-Key: $idempotency_key" \
            -d "{
                \"score\": $random_score,
                \"gameMode\": \"$random_game_mode\",
                \"playerLevel\": $player_level,
                \"trophyCount\": $trophy_count
            }")
        
        # Check if submission was successful
        if echo "$submit_response" | grep -q '"success":true'; then
            ((success_count++))
            echo -n "."
        else
            ((error_count++))
            echo -n "x"
        fi
        
        ((total_scores++))
        
        # Small delay to avoid overwhelming the API
        sleep 0.05
    done
    
    echo ""
    echo "   �� User $username: $scores_count scores submitted"
    echo ""
done

echo ""
echo "🎯 Score Submission Summary:"
echo "   ✅ Successful submissions: $success_count"
echo "   ❌ Failed submissions: $error_count"
echo "   📈 Total attempts: $total_scores"
echo "   🎮 Users processed: 100"
echo "   �� Average scores per user: $((total_scores / 100))"
echo ""
echo "🎉 Score submission completed!"
echo ""
echo "📊 Check your leaderboard:"
echo "   Classic Mode: http://localhost:5088/leaderboard/top?gameMode=Classic&n=10"
echo "   Tournament Mode: http://localhost:5088/leaderboard/top?gameMode=Tournament&n=10"