SRC = $(wildcard $(SRC_DIR)/*.cs) Application.cs
TARGET = $(BUILD_DIR)/robol.exe
SRC_DIR = compiler
BUILD_DIR = build

CSC_FLAGS = -errorendlocation

.PHONY: all
all: $(TARGET)

$(TARGET): $(SRC)
	@mkdir -p $(BUILD_DIR)
	@csc $(SRC) -out:$(TARGET) $(CSC_FLAGS)

.PHONY: run
run: $(TARGET)
	@mono $(TARGET)

clean:
	@rm -rf $(BUILD_DIR)
