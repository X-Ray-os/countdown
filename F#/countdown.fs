module CountdownApp

open System
open System.Text.RegularExpressions
open System.Threading

// ------------------------------------------------------------
// 倒计时实体及操作（修复可变性和实例引用问题）
// ------------------------------------------------------------
type CountDown(name: string, originalTargetDate: DateTime) =
    // 可变字段：当前目标时间（用于运行状态）
    let mutable currentTargetDate = originalTargetDate
    // 可变字段：暂停时的目标时间点
    let mutable remainDate = originalTargetDate
    // 状态：None=未开始/结束, Some true=运行, Some false=暂停
    let mutable status : bool option = None

    member _.Name = name
    // 返回原始目标日期（用于重置）
    member _.OriginalTargetDate = originalTargetDate
    member _.CurrentTargetDate = currentTargetDate
    member _.RemainDate = remainDate
    member _.Status = status

    // 获取当前剩余秒数
    member this.GetRemainSeconds() =
        match status with
        | None -> 0
        | Some true -> int (currentTargetDate - DateTime.Now).TotalSeconds
        | Some false -> int (remainDate - DateTime.Now).TotalSeconds

    // 开始（从未开始或暂停状态转为运行）
    member this.Start() =
        match status with
        | None ->
            status <- Some true
            currentTargetDate <- originalTargetDate   // 重置到原始目标
            remainDate <- originalTargetDate
        | Some false ->
            status <- Some true
            // 保持原有剩余时长，重新计算目标时间点
            let remainingSec = (remainDate - DateTime.Now).TotalSeconds
            if remainingSec > 0.0 then
                currentTargetDate <- DateTime.Now.AddSeconds remainingSec
                remainDate <- currentTargetDate
        | _ -> ()

    // 暂停
    member this.Pause() =
        match status with
        | Some true ->
            status <- Some false
            remainDate <- DateTime.Now.AddSeconds (float (this.GetRemainSeconds()))
        | _ -> ()

    // 重置（回到最初未开始状态）
    member this.Reset() =
        status <- None
        currentTargetDate <- originalTargetDate
        remainDate <- originalTargetDate

    // 完成百分比（0~1）
    member this.Percentage(totalSec: int) : double =
        let total = float totalSec
        if total <= 0.0 then 1.0
        else
            let remain = float (this.GetRemainSeconds())
            Math.Round(1.0 - remain / total, 3)

// ------------------------------------------------------------
// 辅助函数
// ------------------------------------------------------------
let tryParseSpan (raw: string) =
    try
        let raw = raw.Trim().ToLower()
        let mutable weeks, days, hours, minutes, seconds = 0, 0, 0, 0, 0
        let pattern = Regex(@"(\d+)([dhmsw])")
        for m in pattern.Matches(raw) do
            let value = int m.Groups.[1].Value
            match m.Groups.[2].Value with
            | "w" -> weeks <- weeks + value
            | "d" -> days <- days + value
            | "h" -> hours <- hours + value
            | "m" -> minutes <- minutes + value
            | "s" -> seconds <- seconds + value
            | _ -> ()
        let span = TimeSpan(weeks * 7 + days, hours, minutes, seconds)
        if span.TotalSeconds > 0.0 then Some span else None
    with _ -> None

let formatTime (sec: int) =
    let h = sec / 3600
    let m = (sec % 3600) / 60
    let s = sec % 60
    if h > 0 then sprintf "%02d:%02d:%02d" h m s
    else sprintf "%02d:%02d" m s

let showProgress (cd: CountDown) (totalSec: int) (barLength: int) =
    let percent = cd.Percentage(totalSec)
    let filled = int (float barLength * percent)
    let bar = String('■', filled) + String('□', barLength - filled)
    let remainSec = cd.GetRemainSeconds()
    let remainStr = if remainSec <= 0 then "0秒" else formatTime remainSec
    printf "\r进度: [%s] %5.1f%%  剩余: %s" bar (percent * 100.0) remainStr
    stdout.Flush()

// ------------------------------------------------------------
// 倒计时主循环
// ------------------------------------------------------------
let rec countdownLoop (cd: CountDown) (totalSec: int) =
    cd.Start()
    let mutable running = true
    while running && cd.GetRemainSeconds() > 0 do
        try
            showProgress cd totalSec 30
            Thread.Sleep(1000)
        with :? ThreadInterruptedException ->
            running <- false

    if cd.GetRemainSeconds() <= 0 then
        printfn "\n倒计时结束！"
    else
        printfn "\n倒计时已中断"

    // 结束后的命令处理
    let rec commandLoop () =
        printf "\n命令 (c=继续, r=重置并继续, q=退出): "
        match Console.ReadLine() with
        | null -> commandLoop()
        | cmd ->
            match cmd.Trim().ToLower() with
            | "c" ->
                printfn "继续..."
                countdownLoop cd totalSec
            | "r" ->
                cd.Reset()
                printfn "已重置并开始"
                countdownLoop cd totalSec
            | "q" ->
                printfn "退出倒计时"
                ()
            | _ ->
                printfn "无效命令，请输入 c / r / q"
                commandLoop()
    commandLoop()

// ------------------------------------------------------------
// 程序入口
// ------------------------------------------------------------
[<EntryPoint>]
let main argv =
    Console.Write("请输入倒计时名称: ")
    let name = 
        match Console.ReadLine() with
        | null -> "未命名"
        | s when String.IsNullOrWhiteSpace s -> "未命名"
        | s -> s.Trim()

    let rec getSpan() =
        Console.Write("请输入倒计时时长 (例如 1h30m / 2m30s / 1d): ")
        match Console.ReadLine() with
        | null -> getSpan()
        | s -> 
            match tryParseSpan s with
            | Some span -> span
            | None ->
                printfn "时长格式错误，请重新输入"
                getSpan()

    let span = getSpan()
    let totalSec = int span.TotalSeconds
    let targetDate = DateTime.Now.Add span
    let cd = CountDown(name, targetDate)

    printfn $"倒计时 [%s{name}] 开始，总时长: {formatTime totalSec}"
    countdownLoop cd totalSec
    0
