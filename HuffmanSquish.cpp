//////////////////////////////////////////////////////////////
// HuffmanSquish.cpp
// --------
// Part of a Huffman compression assignment. (AIE 2014)
// (see HuffmanSquish.h)
//	
//////////////////////////////////////////////////////////////

#include "HuffmanSquish.h"
#include <iostream>
#include <sstream>
#include <conio.h>
#include <algorithm>
#include <bitset>

using namespace std;

HuffmanSquish::HuffmanSquish() :
	p_raw_file_uncompressed(nullptr),
	p_raw_file_compressed(nullptr),
	p_compressed_data(nullptr),
	p_binary_tree_root_node(nullptr)
{}

HuffmanSquish::~HuffmanSquish()
{ FreeUpResources(); }


void
	HuffmanSquish::Run()
{
	while(true)// Main program loop.
	{
		// Display title //
		cout << "\n";
		cout << "---------------------------------------\n";
		cout << "          ----------------             \n";
		cout << "           Huffman Squish              \n";
		cout << "          ----------------             \n";
		cout << " A simple application demonstrating a  \n";
		cout << "  slightly modified Huffman encoding   \n";
		cout << "  compression/decompression process.   \n";
		cout << "          ----------------             \n";
		cout << "    Develped by Dennis for AIE2014     \n\n";
		cout << "---------------------------------------\n\n";

		// Display prompt for input //
		cout << "---------------------------------------\n";
		cout << "How to use:							\n\n";

		cout << "Drag-and-drop a supported file onto    \n";
		cout << "this console window.					\n\n";

		cout << "(.txt) and (.png)(8bit) will be        \n";
		cout << "compressed.							\n\n";

		cout << "(.squished) will be decompressed.      \n\n\n";



		// Check input //
		while(source_directory.empty())
		{
			// Collect user input.
			source_directory = CollectInput();
		}

		cout << "---------------------------------------\n";
		cout << "(Drag-and-drop successful.)			\n\n";



		// Check for compatable extension.     //
		// Compress, Decompress, or try again. //
		file_type = DetectFileType(source_directory);

		cout << "---------------------------------------\n";
		switch (file_type)
		{
			////////////////////////////////////////////////////////////////////
			//                           COMPRESS                             //
			////////////////////////////////////////////////////////////////////

		case HuffmanSquish::TXT:
		case HuffmanSquish::BMP:
			if(file_type == TXT)
				cout << "Text file detected.				\n\n";
			if(file_type == BMP)
				cout << "BitMap file detected.             \n\n";

			// Read file into memory as uncompressed data //
			p_raw_file_uncompressed = ReadFileIntoMemory(source_directory);

			// Display error if file could not be loaded //
			if(p_raw_file_uncompressed == nullptr)
			{
				cout << "---------------------------------------\n";
				cout << "File could not be loaded.				\n";
				cout << "Check it's not in use, and that the    \n";
				cout << "file size is greater than 4bytes and   \n";
				cout << "try again... bitch.					\n\n";
				continue;
			}

			cout << "---------------------------------------\n";
			cout << "(File loaded successfully.)			\n\n";



			// Translate uncompressed data into a vector of leaf-nodes //
			cout << "---------------------------------------\n";
			cout << "Generating leaf nodes:";
			ordered_leaf_nodes = ExtractNodesFromFile(p_raw_file_uncompressed);

			// Display leaf node 'value' and 'count' //
			printf("\n%-10s%-10s%-10s%\n", "Value", "Char", "Count"); 
			for(unsigned int i = 0; i < ordered_leaf_nodes.size(); i++)
				printf("%-10d%-10c%-10d\n", 
				(unsigned short)(unsigned char)ordered_leaf_nodes[i]->value,
				ordered_leaf_nodes[i]->value,
				ordered_leaf_nodes[i]->count);
			cout << "\n\n";



			// Construct a binary tree to hold the leaf nodes //
			cout << "---------------------------------------\n";
			cout << "Generating binary tree..........		\n";
			p_binary_tree_root_node = GenerateBinaryTree(ordered_leaf_nodes);
			cout << "(Tree generated successfully.)			\n\n";



			// Create binary path for each leaf-node //
			cout << "---------------------------------------\n";
			cout << "Generating binary paths..........		\n";
			GenerateBinaryPaths(p_binary_tree_root_node);

			// Display binary paths //
			printf("\n%-10s%-10s%-10s%-10s\n", "Value", "Char", "Count", "Path"); 
			for(unsigned int i = 0; i < ordered_leaf_nodes.size(); i++)
				printf("%-10d%-10c%-10d%-10s\n", 
				(unsigned short)(unsigned char)ordered_leaf_nodes[i]->value,
				ordered_leaf_nodes[i]->value,
				ordered_leaf_nodes[i]->count,
				ordered_leaf_nodes[i]->path.c_str());
			cout << "\n\n";



			// Compress data //
			cout << "---------------------------------------\n";
			cout << "Compressing data:";
			p_compressed_data = CompressLeafData(ordered_leaf_nodes, p_raw_file_uncompressed);
			cout << "\n(Data compressed.)					\n\n";


			// Save data to disc //
			cout << "---------------------------------------\n";
			cout << "Saving compressed data..........		\n";
			SaveCompressedData(p_compressed_data, source_directory);
			cout << "(Saved.)								\n\n";

			break;



			////////////////////////////////////////////////////////////////////
			//                           DECOMPRESS                           //
			////////////////////////////////////////////////////////////////////
		case HuffmanSquish::SQUISHED:
			cout << "Squished file detected.			\n\n";

			// Read file into memory as uncompressed data //
			p_raw_file_compressed = ReadFileIntoMemory(source_directory);

			// Display error if file could not be loaded //
			if(p_raw_file_compressed == nullptr)
			{
				cout << "---------------------------------------\n";
				cout << "File could not be loaded.				\n";
				cout << "Check it's not in use, and that the    \n";
				cout << "file size is greater than 4bytes and   \n";
				cout << "try again... bitch.					\n\n";
				continue;
			}

			cout << "---------------------------------------\n";
			cout << "(File loaded successfully.)			\n\n";



			// Translate raw-compressed-file into compressed file //
			cout << "---------------------------------------\n";
			cout << "Turning raw compressed file to			\n";
			cout << "readable compressed file..........		\n";
			p_compressed_data = TranslateRawCompressedFile(p_raw_file_compressed);
			cout << "(Successful.)								\n\n";


			// Translate compressed-file into a vector of leaf-nodes //
			cout << "---------------------------------------\n";
			cout << "Extracting leaf nodes..........		\n";
			ordered_leaf_nodes = ExtractNodesFromCompressedFile(p_compressed_data);

			// Display leaf node 'value' and 'count' //
			printf("\n%-10s%-10s%-10s%\n", "Value", "Char", "Count"); 
			for(unsigned int i = 0; i < ordered_leaf_nodes.size(); i++)
				printf("%-10d%-10c%-10d\n", 
				(unsigned short)(unsigned char)ordered_leaf_nodes[i]->value,
				ordered_leaf_nodes[i]->value,
				ordered_leaf_nodes[i]->count);
			cout << "\n\n";



			// Construct a binary tree to hold the leaf nodes //
			cout << "---------------------------------------\n";
			cout << "Generating binary tree..........		\n";
			p_binary_tree_root_node = GenerateBinaryTree(ordered_leaf_nodes);
			cout << "(Tree generated successfully.)			\n\n";



			// De-compress data using tree //
			cout << "---------------------------------------\n";
			cout << "Decompressing file data:";
			p_raw_file_uncompressed = DecompressFileData(p_binary_tree_root_node, p_compressed_data);
			cout << "\n(Decompressed successfully.)			\n\n";



			// Save data to disc //
			cout << "---------------------------------------\n";
			cout << "Saving file to disk..........			\n";
			SaveRawData(p_raw_file_uncompressed, source_directory);
			cout << "(Saved Successfully.)			\n\n";

			break;



			////////////////////////////////////////////////////////////////////
			//                       Not compatable                           //
			////////////////////////////////////////////////////////////////////
		case HuffmanSquish::NOTSUPPORTED:
			cout << "File extension not supported.		\n";
			cout << "Try again... bitch.				\n\n";
		}




		// Go round again //
		cout << endl;
		system("pause");
		system("cls");
		FreeUpResources();

	};// Main program loop.
}



string
	HuffmanSquish::CollectInput()
{
	// Collect console input.
	// - _getch() input characters until two quotation marks are found.
	// - reject if input doesn't start with a quotation mark (").
	// - return string contained inside "...".

	stringstream ss;
	char ca;
	int quoteCount = 0;

	while (quoteCount != 2)
	{
		ca = _getch();

		if(ca == '\"'){
			quoteCount++;
			continue;
		}

		if(quoteCount == 0)
			return "";

		ss << ca;
	};

	return ss.str().c_str();
}


HuffmanSquish::SupportedFileTypes  
	HuffmanSquish::DetectFileType(string a_file_path)
{
	// Abort if there's no file extension.
	if (a_file_path.find_last_of(".") == string::npos)
		return HuffmanSquish::NOTSUPPORTED;

	// '.txt' extension.
	if(a_file_path.substr(a_file_path.find_last_of(".")+1) == "txt")
		return HuffmanSquish::TXT;

	// '.png' extension.
	if(a_file_path.substr(a_file_path.find_last_of(".")+1) == "bmp")
		return HuffmanSquish::BMP;

	// '.squished' extension.
	if(a_file_path.substr(a_file_path.find_last_of(".")+1) == "squished")
		return HuffmanSquish::SQUISHED;

	// Abort if no compatible extension found.
	return HuffmanSquish::NOTSUPPORTED;
}


HuffmanSquish::RawData*
	HuffmanSquish::ReadFileIntoMemory(string a_file_path)
{
	// Open file 
	fstream mfile_stream(source_directory.c_str(), ios_base::in | ios_base::binary | ios_base::ate);

	// Read file if open //
	if(mfile_stream.is_open())
	{
		RawData* mptemp = new RawData();

		// Get file size
		mptemp->size = (int)mfile_stream.tellg();
		// Reserve memory on heap
		mptemp->memory_block = new char[mptemp->size];
		// Set stream to beginning of file
		mfile_stream.seekg(0, ios::beg);
		// Read file into memory
		mfile_stream.read(mptemp->memory_block, mptemp->size);
		// Close file.
		mfile_stream.close();

		// Minimum file size requirement = 4bytes.
		if(mptemp->size < 4) return nullptr;

		// Return Uncompressed file
		return mptemp;
	}

	// Retun nullpointer if file can't be opened
	return nullptr;
}


vector<HuffmanSquish::Node*>
	HuffmanSquish::ExtractNodesFromFile( HuffmanSquish::RawData* ap_raw_file)
{
	int mprogress = 0;

	// Populate a vector of leaf nodes with value and count //
	vector<Node*> mreapeating_values;
	for(int i = 0; i < ap_raw_file->size; i++)
	{
		// Check if data is alreay contained in a leaf node //
		bool mfound = false;
		unsigned int j = 0;
		for(; j < mreapeating_values.size(); j++)
		{
			if(ap_raw_file->memory_block[i] == mreapeating_values[j]->value)
			{
				mfound = true;
				break;
			}
		}


		// If already in leaf node vector
		if(mfound)
		{
			// Increase count.
			mreapeating_values[j]->count++;
		}

		// If not in leaf node vector
		else
		{
			// Add new leaf node
			Node* mnode = new Node();
			mnode->value = ap_raw_file->memory_block[i];
			mnode->count = 1;
			mreapeating_values.push_back(mnode);
		}

		// Display progress bar
		if(i > mprogress)
		{
			mprogress += (int)(ap_raw_file->size * .1f);
			cout << ".";
		}
	}

	// Sort vector by Node.count (decending numerical)//
	sort(mreapeating_values.begin(), mreapeating_values.end(),
		[](Node* a, Node* b){ return a->count < b->count; });


	return mreapeating_values;
}


HuffmanSquish::CompressedData*
	HuffmanSquish::TranslateRawCompressedFile(RawData* ap_raw_compressed_file)
{
	CompressedData* cd = new CompressedData();

	// Get element count.
	cd->element_count = *(int*)ap_raw_compressed_file->memory_block;

	// Get values array
	int position_handle = sizeof(int);
	cd->values = &ap_raw_compressed_file->memory_block[position_handle];

	// Get value counts
	position_handle += cd->element_count;
	cd->value_counts = (int*)&ap_raw_compressed_file->memory_block[position_handle];

	// Get path size
	position_handle += cd->element_count* sizeof(int);
	cd->binary_path_size = *(int*)&ap_raw_compressed_file->memory_block[position_handle];

	// Get path
	position_handle += sizeof(int);
	cd->binary_paths_sequence = &ap_raw_compressed_file->memory_block[position_handle];

	return cd;
}


vector<HuffmanSquish::Node*>
	HuffmanSquish::ExtractNodesFromCompressedFile( HuffmanSquish::CompressedData* ap_raw_file)
{

	// size from first 4bytes of header
	int element_count = ap_raw_file->element_count;

	// Populate a vector of leaf nodes with value and count //
	vector<Node*> leaf_nodes;
	for(int i = 0; i < element_count; i++)
	{
		Node* n = new Node();
		n->value = ap_raw_file->values[i];
		n->count = ap_raw_file->value_counts[i];
		leaf_nodes.push_back(n);
	}

	return leaf_nodes;
}



// TODO: devise a more blanced method for generating a tree.
HuffmanSquish::Node* 
	HuffmanSquish::GenerateBinaryTree(vector<Node*> a_leaf_nodes)
{
	// Create binary tree.
	// Set first two leaf-nodes.
	// Cycle through remaining leaf-nodes.
	// Instert them into tree.


	// Link lowest-count leaf-nodes to tree and remove them from vector //

	unsigned int i = 0;
	// Create root-node.
	Node* root_node = new Node();
	// Set left-node.
	root_node->left_node = a_leaf_nodes[i];
	// Set left-node's parent.
	root_node->left_node->parent_node = root_node;
	// Iterator to next element.
	i++;
	// Set right-node.
	root_node->right_node = a_leaf_nodes[i];
	// Set right-node's parent.
	root_node->right_node->parent_node = root_node;
	// Iterator to next element.
	i++;

	// Set count of root-node as total of its children.
	root_node->count = 
		root_node->left_node->count + root_node->right_node->count;



	// Insert all remaning leaf nodes //

	Node* search_node;
	for(; i < a_leaf_nodes.size();)
	{
		bool insertion_point_found = false;
		// Begin search at root.
		search_node = root_node;


		// Set 'search-node' to the first node with a lower 'count' than the current leaf-node //
		while (!insertion_point_found)
		{
			// Check if the search-node is lower //
			if( a_leaf_nodes[i]->count >= search_node->count )
			{
				insertion_point_found = true;
				continue;
			}

			// If not, check lowest child
			if(search_node->left_node->count < search_node->right_node->count)
			{
				if( a_leaf_nodes[i]->count >= search_node->left_node->count )
				{
					search_node = search_node->left_node;
					insertion_point_found = true;
					continue;
				}
			}
			else
			{
				if( a_leaf_nodes[i]->count >= search_node->right_node->count )
				{
					search_node = search_node->right_node;
					insertion_point_found = true;
					continue;
				}
			}

			// If still not, navigate to lowest child.
			if(search_node->left_node->count < search_node->right_node->count)
			{
				search_node = search_node->left_node;
			}
			else
			{
				search_node = search_node->right_node;
			}

		};


		// Insert leaf-node into the tree at search-nodes position //

		// Create new branch node.
		Node* branch = new Node();

		// If search-node is the root-node.
		if(search_node == root_node)
		{
			// set the new branch as the new root.
			root_node = branch;
		}
		// Otherwise 
		else
		{
			// Set the new branch's parent to search-node's parent.
			branch->parent_node = search_node->parent_node;
			// Set child.
			if(branch->parent_node->left_node == search_node)
				branch->parent_node->left_node = branch;
			else if(branch->parent_node->right_node == search_node)
				branch->parent_node->right_node = branch;
			else
				throw exception("done gone fucked up");
		}

		// Set the new branch's left-node to the search-node.
		branch->left_node = search_node;
		// Set parent
		branch->left_node->parent_node = branch;
		// Set the branch's right-node to the leaf-node.
		branch->right_node = a_leaf_nodes[i];
		// Set parent
		branch->right_node->parent_node = branch;
		// Iterator to next element.
		i++;

		// Update all count for this and all parent nodes.
		Node* count_updater = branch;
		while(count_updater != nullptr)
		{
			count_updater->count = 
				count_updater->left_node->count + count_updater->right_node->count;
			count_updater = count_updater->parent_node;
		}
	};

	return root_node;
}


void
	HuffmanSquish::GenerateBinaryPaths(Node* a_root_node)
{
	// Navigate down the tree assigning path to leaf-node //
	// Navigate left moving down the tree.
	// Record path.
	// Assign path when leaf-node reached.
	// Navigate right if not already done so on the way up.

	string current_path = "";
	Node* navigator_node = a_root_node;
	bool going_down = true;
	bool returned_to_root = false;

	while (navigator_node != nullptr)
	{
		// If navigating down
		if(going_down)
		{
			// If navigator node is a leaf-node.
			if(navigator_node->left_node == nullptr && navigator_node->right_node == nullptr)
			{
				// Set path inside leaf-node
				navigator_node->path.assign(current_path);
				// Set to navigate back up.
				going_down = false;

				// Navigate to parent node.
				navigator_node = navigator_node->parent_node;
				//current_path.pop_back();
			}

			// If it's not a leaf-node.
			else
			{	
				// Navigate down left.
				navigator_node = navigator_node->left_node;
				// Update path.
				current_path.append("0");
			}
		}

		// If navigating up
		else
		{
			// If haven't navigated down right of parent node.
			if(current_path.back() == '0')
			{ 
				current_path.pop_back();
				// Then navigate down right.
				navigator_node = navigator_node->right_node;
				current_path.append("1");
				going_down = true;
			}
			// Otherwise 
			else
			{
				// Keep going up
				navigator_node = navigator_node->parent_node;
				current_path.pop_back();
			}
		}
	};
}


HuffmanSquish::CompressedData*
	HuffmanSquish::CompressLeafData(vector<Node*> a_leaf_nodes, RawData* a_unp_compressed_data)
{
	//	First 4bytes, representing number of <value,count> pairs.
	//  Then pairs represented as two arrays:
	//	> value[] count[]

	//  Then the size of the file data in bytes.
	//	Then file data represented as paths squshed together.
	//  > 100101010010010010...etc


	// Header //

	// New compressed data
	CompressedData* p_compressed_data = new CompressedData();
	// Set element count
	p_compressed_data->element_count = a_leaf_nodes.size();
	// Create array of 'values'
	p_compressed_data->values = new char[p_compressed_data->element_count];
	for(int i = 0; i < p_compressed_data->element_count; i++)
		p_compressed_data->values[i] = a_leaf_nodes[i]->value;
	// Create array of 'counts'
	p_compressed_data->value_counts = new int[p_compressed_data->element_count];
	for(int i = 0; i < p_compressed_data->element_count; i++)
		p_compressed_data->value_counts[i] = a_leaf_nodes[i]->count;


	// Body //

	// Represent each byte of the uncompressed file a binary path	//
	// and smash those paths together into one long string.			//

	int mprogress = 0;
	string all_paths;
	// Get byte of data.
	for(int i = 0; i < a_unp_compressed_data->size; i++)
	{
		// Find data in leaf nodes
		for(unsigned int j = 0; j < a_leaf_nodes.size(); j++)
		{
			if(a_leaf_nodes[j]->value == a_unp_compressed_data->memory_block[i])
			{
				// Add path from leaf-node to string.
				all_paths.append(a_leaf_nodes[j]->path);
				break;
			}
		}
		// Display progress bar
		if(i > mprogress)
		{
			mprogress += int(a_unp_compressed_data->size * .1f);
			cout << ".";
		}
	}

	// Convert each eight binary characters of string to new char	//
	// and add to compressed data array.							//

	// Create array for newly created chars
	p_compressed_data->binary_path_size = (all_paths.size() / 8) + 1;
	p_compressed_data->binary_paths_sequence = new char[p_compressed_data->binary_path_size];
	// Turn every 8 binary chars to a bitset
	for(unsigned int i = 0, k = 0; i < all_paths.size();)
	{
		bitset<8> bits;
		for(int j = 0; j < 8; j++)
		{
			if( i >= all_paths.size() ) 
				break;
			if(all_paths[i] == '0')
				bits.set(j, 0);
			else if(all_paths[i] == '1')
				bits.set(j,1);
			else
				throw exception("Error reading uncompressed data!");
			i++;
		}

		// Turn bitset to char.
		unsigned long ul = (unsigned long)bits.to_ullong();
		unsigned char c = static_cast<unsigned char>(ul);

		// Add char to compressed data
		p_compressed_data->binary_paths_sequence[k] = c;
		k++;
	};

	return p_compressed_data;
}


HuffmanSquish::RawData*
	HuffmanSquish::DecompressFileData(Node* ap_binary_tree_root_node, CompressedData* ap_compressed_data)
{
	// Repeat until the end of the binary-path is reached:-
	//	 Extract a byte of data from binary-paths.
	//	 Navigate down the binary-tree using the 1's and 0's from the byte.
	//	 If a leaf-node is reached, add the value to the decompressed-data.
	// Return raw-file created from the decompressed data.


	vector<char> decompressed_data;
	Node* search_node = ap_binary_tree_root_node;
	int mprogress = 0;

	// Translate all binary paths into bytes //
	for(int i = 0; i < ap_compressed_data->binary_path_size; i++)
	{
		// Extract byte
		bitset<8> byte = bitset<8>(p_compressed_data->binary_paths_sequence[i]);

		for(int j = 0; j < 8; j++)
		{
			// Traverse binary-tree using bits in byte //
			// Go right
			if(byte[j] == 1)
				search_node = search_node->right_node;
			// Go left
			else if(byte[j] == 0)
				search_node = search_node->left_node;
			else
				throw new exception("Path navigation fucked up!");

			// If leaf-node reached
			if(search_node->left_node == nullptr && search_node->right_node == nullptr)
			{
				if( search_node->count > 0 )
				{
					// Add leaf-node value to decompressed data.
					decompressed_data.push_back(search_node->value);
					// Reduce leaf-node count
					search_node->count--;
				}

				// Set search node back to start.
				search_node = ap_binary_tree_root_node;
			}
		}

		// Display progress bar
		if(i > mprogress)
		{
			mprogress += (int)(ap_compressed_data->binary_path_size * .1f);
			cout << ".";
		}
	}


	// Return decoded data as raw-file //

	RawData* rf = new RawData();
	rf->size = decompressed_data.size();

	// Turn char-vector to array.
	char* dd = new char[decompressed_data.size()];
	for(unsigned int i = 0; i < decompressed_data.size(); i++)
		dd[i] = decompressed_data[i];
	rf->memory_block = dd; 

	// Return raw-file.
	return rf;
}


void
	HuffmanSquish::SaveCompressedData(CompressedData* ap_compressed_data, string a_source_directory)
{
	// File structure :-
	//	int		- element_count ------- (number of [value,count] sets)
	//	char[]	- values -------------- (paired with 'value_count')
	//	int[]	- value_count ---------	(how many times each value is found)
	//	int		- binary_path_size ---- (size in bytes of binary path)
	//	char[]	- binary_path_sequence  (binary sequence saved as char[], eg. 0001101110100101 = char[0]{00011011} char[1]{10100101})

	a_source_directory.append(".squished");
	ofstream file;
	file.open(a_source_directory, ios::binary | ios::out);
	file.write(reinterpret_cast<const char*>(&ap_compressed_data->element_count), sizeof(int));
	file.write(ap_compressed_data->values, sizeof(char) * ap_compressed_data->element_count);
	file.write(reinterpret_cast<const char*>(ap_compressed_data->value_counts), sizeof(int) * ap_compressed_data->element_count);
	file.write(reinterpret_cast<const char*>(&ap_compressed_data->binary_path_size), sizeof(int));
	file.write(ap_compressed_data->binary_paths_sequence, sizeof(char) * ap_compressed_data->binary_path_size);
	file.close();
}


void
	HuffmanSquish::SaveRawData(HuffmanSquish::RawData* ap_raw_data, string a_source_directory)
{
	// Remove ".squished" suffix from source directory.
	a_source_directory = a_source_directory.substr(0, a_source_directory.size() - 9 /*.squished*/);

	// Create/Replace file on disk.
	ofstream file;
	file.open(a_source_directory, ios::binary | ios::out);

	// Write data to file.
	file.write(ap_raw_data->memory_block, ap_raw_data->size);

	// close file
	file.close();
}


void
	HuffmanSquish::FreeUpResources()
{
	// Cleans up used memory. //

	source_directory = "";
	ordered_leaf_nodes.clear();

	if(p_raw_file_uncompressed != nullptr)
	{
		delete p_raw_file_uncompressed;
		p_raw_file_uncompressed = nullptr;
	}

	if(p_raw_file_compressed != nullptr)
	{
		delete p_raw_file_compressed;
		p_raw_file_compressed = nullptr;
	}

	if(p_compressed_data != nullptr)
	{
		delete p_compressed_data;
		p_compressed_data = nullptr;
	}

	if(p_binary_tree_root_node != nullptr)
	{
		delete p_binary_tree_root_node;
		p_binary_tree_root_node = nullptr;
	}
}



HuffmanSquish::Node::Node() :
	left_node(nullptr),
	right_node(nullptr),
	parent_node(nullptr),
	value(NULL)
{}

