﻿module FrameActivePatternsTests

open Expecto
open faprs.core.FrameActivePatterns
open System

[<Literal>]
let CHANNEL_ONLY_RECORD = "[0]"

[<Literal>]
let EMPTY_RECORD = ""

let GARBAGE_RECORD = " AB CD:ABC DABCD]"

[<Literal>]
let GOOD_POSITION_REPORT_RECORD = "[0] KG7SIO-7>APRD15,WIDE1-1,TCPXX*,qAX,CWOP-2:=3216.4N/11057.3Wbblah:blah /fishcakes"
[<Literal>]
let GOOD_ADDRESS_MESSAGE = "[0] KG7SIO-7>APRD15,WIDE1-1,TCPXX*,qAX,CWOP-2:=3216.4N/11057.3Wb,b>,lah:blah /fishcakes"
[<Literal>]
let GOOD_MESSAGE = "=3216.4N/11057.3Wb,b>,lah:blah /fishcakes"
[<Literal>]
let BAD_REPORT = "=3216.4N'11057.3Wb,b>,lah:blah /fishcakes"
[<Literal>]
let BAD_ADDRESS_MESSAGE = "[0] KG7SIO-7+APRD15-WIDE1-CPXX*,qAX,CWOP-2:=3216.4N/11057.3Wb>,b>,lah:blah /fishcakes"
[<Literal>]
let GOOD_ADDRESS = "KG7SIO-7>APRD15,WIDE1-1,TCPXX*,qAX,CWOP-2"
[<Literal>]
let GOOD_ADDRESS_MULTI_PATH = "KG7SIO-7>APRD15,WIDE1-1,TCPXX*,qAX,CWOP-2"
[<Literal>]
let BAD_ADDRESS = "KG7SIO-7+APRD15-WIDE1-CPXX*,qAX,CWOP-2"
[<Literal>]
let GOOD_FRAME = "KG7SIO-7>APRD15,WIDE1-1,TCPXX*,qAX,CWOP-2:=3216.4N/11057.3Wbblah:blah /fishcakes"
[<Literal>]
let BAD_LOCATION = "[0] KG7SIO-7>APRD15,WIDE1-1:=3216.4/11057.3bblah:blah /fishcakes"
[<Literal>]
let NO_COMMENT = "[0] KG7SIO-7>APRD15,WIDE1-1:=3216.4N/11057.3Wb"
//[<Literal>]
//let BAD_ADDRESS = "[0] >APRD15,WIDE1-1:=3216.4N/11057.3Wbblah:blah /fishcakes"

[<Tests>]
let FrameParsingTests =
    testList "Parse kiss util frames" [
        testCase "Good record returns a good frame" <| fun _ -> 
            let frame = 
                match GOOD_POSITION_REPORT_RECORD with
                | Frame f -> f
                | _ -> failwith "Could not match frame. Check format."
            Expect.equal frame GOOD_FRAME "Could not parse frame" 
        testCase "Empty record should return None" <| fun _ ->
            let result =
                match EMPTY_RECORD with
                | Frame f -> Some f
                | _ -> None
            Expect.isNone result "Empty record should have returned None"
        testCase "Channel only record should return None" <| fun _ ->
            let result = 
                match CHANNEL_ONLY_RECORD with
                | Frame f -> Some f
                | _ -> None
            Expect.isNone result "Channel-only record should have returned None"
    ]

[<Tests>]
let AddressParsingTests =
    testList "Parse address elements" [
        testCase "Can parse address in good record with message" <| fun _ ->
            let frame = 
                match GOOD_ADDRESS_MESSAGE with
                | Frame f -> f
                | _ -> failwith "Could not match frame. Check format."
            let address =
                match frame with
                | Address a -> a
                | _ -> failwith "Could not match address. Check format."
            Expect.equal address GOOD_ADDRESS_MULTI_PATH "Address was not parsed correctly"
        testCase "Can parse sender in good address" <| fun _ ->
            let sender =
                match GOOD_ADDRESS with
                | Sender s -> s
                | _ -> String.Empty
            Expect.equal sender "KG7SIO-7" "Sender did not match."
        testCase "Sender in malformed address is not parsed" <| fun _ ->
            let address = 
                match BAD_ADDRESS with
                | Address a -> Some a
                | _ -> None
            Expect.isNone address "Address should not have been parsed."  
        testCase "Can parse Destination in good address" <| fun _ ->
            let destination =
                match GOOD_ADDRESS with
                | Destination d -> d
                | _ -> String.Empty
            Expect.equal destination "APRD15" "Destination did not match."
        testCase "Destination in malformed address is not parsed" <| fun _ ->
            let destination =
                match BAD_ADDRESS with
                | Destination d -> Some d
                | _ -> None
            Expect.isNone destination "Destination should not have been parsed"
        testCase "Can parse Path in good address" <| fun _ ->
            let path =
                match GOOD_ADDRESS with
                | Path p -> p |> Array.toSeq
                | _ -> Seq.empty
            Expect.containsAll path (["WIDE1-1"; "TCPXX*"; "qAX"; "CWOP-2"] |> List.toSeq)  "Did not contain expected paths"   
            //(["WIDE1-1"; "TCPXX*"; "qAX"; "CWOP-2"] |> List.toSeq)             
            //|> List.iter (fun p -> Expect.contains path p "Path not found in expected paths list")
        testCase "Path in malformed address is not parsed" <| fun _ ->
            let path =
                match BAD_ADDRESS with
                | Path p -> Some (p |> Array.toSeq)
                | _ -> None 
            Expect.isNone path "Path should not have been parsed"
    ]

[<Tests>]
let MessageParsingTests =
    testList "Message Parsing Tests" [
        testCase "Can get message part of well formed frame with message" <| fun _ ->
            let message =
                match GOOD_ADDRESS_MESSAGE with
                | Message m -> m
                | _ -> String.Empty
            Expect.equal message GOOD_MESSAGE "Message does not match"
        testCase "Can get Latitude from well formed message position report" <| fun _ ->
            let result =
                match GOOD_MESSAGE with
                | Latitude l -> l
                | _ -> String.Empty
            Expect.equal result "3216.4N" "Latitude did not match"
        testCase "Latitude in malformed position report cannot be parsed" <| fun _ ->
            let result =
                match BAD_REPORT with
                | Latitude l -> Some l
                | _ -> None
            Expect.isNone result "Latitude should not have been parsed"
        testCase "Can get Longitude from well formed position report" <| fun _ -> 
            let result =
                match GOOD_MESSAGE with
                | Longitude l -> l
                | _ -> String.Empty
            Expect.equal result "11057.3W" "Longitude did not match"
        testCase "Longitude in malformed position report cannot be parsed" <| fun _ ->
            let result =
                match "-3216.4W/11057.3Sb,b>,lah:blah /fishcakes" with
                | Longitude l -> Some l
                | _ -> None
            Expect.isNone result "Longitude should not have been parsed."
        testCase "Can get Symbol from well formed position report" <| fun _ ->
            let result =
                match "=3216.4N/11057.3Eb,b>,lah:blah /fishcakes" with
                | Symbol s -> s
                | _ -> ' '
            Expect.equal result 'b' "Symbol did not match"
        testCase "Symbol in malformed position report cannot be parsed" <| fun _ ->
            let result =
                match "=3216.4I`11057.3Lb,b>,lah:blah /fishcakes" with
                | Symbol b -> Some b
                | _ -> None
            Expect.isNone result "Symbol should not have been parsed"
        //testCase "Can parse well formed position report in well formed frame" <| fun _ ->
        //    let 
    ]
