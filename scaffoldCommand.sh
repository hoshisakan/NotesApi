#!/bin/bash

# 使用方式說明
if [ -z "$1" ]; then
  echo "請輸入參數，例如：./scaffold.sh Dev"
  exit 1
fi

# 根據參數選擇資料庫
if [ "$1" == "Dev" ]; then
  echo "執行 Dev 對應 notesdb_test 資料庫 Scaffold..."
  dotnet ef dbcontext scaffold "Host=172.20.10.9;Port=5433;Database=notesdb_test;Username=root;Password=Shirushi398610" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -f
elif [ "$1" == "Prod" ]; then
  echo "執行 Prod 對應 notesdb_dev 資料庫 Scaffold..."
  dotnet ef dbcontext scaffold "Host=172.34.0.3;Port=5432;Database=notesdb_dev;Username=root;Password=Shirushi398610" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -f
else
  echo "未知的參數：$1，請輸入 Dev 或 Prod"
  exit 1
fi