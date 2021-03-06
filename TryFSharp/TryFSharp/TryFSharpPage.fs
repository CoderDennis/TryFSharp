﻿namespace TryFSharp

open System
open Xamarin.Forms
open Xamarin.Forms.Xaml
open Demo.Numerics

type TryFSharpPage() =
    inherit ContentPage()

    let _ = base.LoadFromXaml(typeof<TryFSharpPage>)

    let image = base.FindByName<Image>("image")
    let progressBar = base.FindByName<ProgressBar>("progressBar")

    let rows = 1000
    let cols = 1000

    let center = Complex(-0.75, 0.0)
    let width = 2.5
    let height = 2.5

    let ColToX col = center.Real - width / 2.0 + float(col) * width / float(cols)
    let RowToY row = center.Imaginary - height / 2.0 + float(row) * height / float(rows)

    let MandelbrotIndex c iterations =
        let rec RecursiveMandelbrot (z : Complex) countdown =
            if countdown = 0 then
                -1
            elif z.Magnitude >= 2.0 then
                iterations - countdown
            else
                RecursiveMandelbrot (z * z + c) (countdown - 1)
        RecursiveMandelbrot (Complex()) iterations

    let CalculateAsync() =
        async {
            return Bmp.Create rows cols (fun row col ->
                
                if (col = 0 && float row % float (rows / 100) = 0.0) then
                    Device.BeginInvokeOnMainThread(fun _ ->
                        do progressBar.Progress <- float row / float rows)

                let x = ColToX col
                let y = RowToY row
                let c = Complex(x, y)
                match MandelbrotIndex c 100 with
                    | -1 -> Color.Black
                    | index -> Color.FromHsla(float index / 64.0 % 1.0, 1.0, 0.5)
                )
        }

    member this.OnCalculateButtonClicked(sender : Object, args : EventArgs) =
        async {
            let button = sender :?> Button
            do button.IsEnabled <- false

            let! token = Async.StartChild(CalculateAsync())
            let! stream = token

            do image.Source <- ImageSource.FromStream(fun _ -> stream)

            do button.IsEnabled <- true
        } |> Async.StartImmediate

type App() =
    inherit Application(MainPage = TryFSharpPage())
