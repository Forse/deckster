cd src/Deckster.Idiot.SampleClient

case $1 in
  --build)
  dotnet build
  ;;
  --clean)
  dotnet clean
  dotnet build
  ;;
  *)
esac

dotnet run $@
