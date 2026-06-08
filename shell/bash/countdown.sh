#!/usr/bin/env bash
set -euo pipefail

parse_span() {
    local raw="$1"
    raw="${raw,,}"
    local seconds=0

    if [[ "$raw" =~ ^[0-9]+$ ]]; then
        seconds=$raw
    else
        local re='([0-9]+)([hms])'
        while [[ "$raw" =~ $re ]]; do
            local value=${BASH_REMATCH[1]}
            local unit=${BASH_REMATCH[2]}
            case "$unit" in
                h) seconds=$((seconds + value * 3600)) ;;
                m) seconds=$((seconds + value * 60)) ;;
                s) seconds=$((seconds + value)) ;;
            esac
            raw=${raw#*${BASH_REMATCH[0]}}
        done
    fi

    printf '%s' "$seconds"
}

print_brick() {
    local past=$1
    local remain=$2
    local total=$3
    local blocks=25
    local filled=0

    if (( total > 0 )); then
        filled=$(( (past * blocks + total / 2) / total ))
    fi
    if (( filled < 0 )); then
        filled=0
    elif (( filled > blocks )); then
        filled=$blocks
    fi

    local bar_filled bar_empty
    bar_filled=$(printf '%*s' "$filled" '' | tr ' ' '#')
    bar_empty=$(printf '%*s' "$((blocks - filled))" '' | tr ' ' '-')

    clear
    echo "|${bar_filled}${bar_empty}|"
    echo "名称: $name"
    echo "总时长: $total 秒"
    echo "已过: $past 秒  剩余: $remain 秒"
    if command -v date >/dev/null 2>&1; then
        local deadline
        if date --version >/dev/null 2>&1; then
            deadline=$(date -d "now + ${remain} seconds" '+%F %T')
        else
            deadline=$(date -v +${remain}S '+%F %T' 2>/dev/null || echo "无法计算")
        fi
        echo "截止时间: $deadline"
    fi
}

echo -n "请输入倒计时名称: "
read -r name

span=0
while (( span <= 0 )); do
    echo -n "请输入倒计时时长 (例如 1h30m, 45s, 90): "
    read -r raw
    span=$(parse_span "$raw")
    if (( span <= 0 )); then
        echo "无效的时间格式，请重新输入。"
    fi
done

total=$span

while (( span > 0 )); do
    local past=$((total - span))
    print_brick "$past" "$span" "$total"
    sleep 1
    span=$((span - 1))
done

print_brick "$total" 0 "$total"
echo "倒计时已结束！"
printf '\a'
