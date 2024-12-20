cd src/Deckster.Server

case $1 in
  --build)
  dotnet build
  dotnet run
  ;;
  --clean)
  dotnet clean
  dotnet build
  dotnet run
  ;;
  *)
  dotnet run
esac