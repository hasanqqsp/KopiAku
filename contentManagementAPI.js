// Content Management API Functions
// This file contains all GraphQL queries and mutations for managing content

// Note: Make sure to import axiosWithAuth in your application:
// import { axiosWithAuth } from './path/to/your/axios/config';

// =============================================================================
// HERO CONTENT FUNCTIONS
// =============================================================================

export async function getHeroContents(variables = {}) {
  const query = `
    query GetHeroContents($first: Int, $after: String, $where: HeroContentFilterInput, $order: [HeroContentSortInput!]) {
      heroContents(first: $first, after: $after, where: $where, order: $order) {
        nodes {
          id
          title
          description
          backgroundImageUrl
          createdAt
          updatedAt
        }
        pageInfo {
          hasNextPage
          hasPreviousPage
          startCursor
          endCursor
        }
        totalCount
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables,
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.heroContents;
  } catch (err) {
    console.error("getHeroContents error:", err);
    throw err;
  }
}

export async function getActiveHeroContent() {
  const query = `
    query GetActiveHeroContent {
      activeHeroContent {
        id
        title
        description
        backgroundImageUrl
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables: {},
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.activeHeroContent;
  } catch (err) {
    console.error("getActiveHeroContent error:", err);
    throw err;
  }
}

export async function updateOrCreateHeroContent(input) {
  const mutation = `
    mutation UpdateOrCreateHeroContent($input: UpdateHeroContentInput!, $backgroundImage: Upload) {
      updateOrCreateHeroContent(input: $input, backgroundImage: $backgroundImage) {
        id
        title
        description
        backgroundImageUrl
        createdAt
        updatedAt
      }
    }
  `;

  // Check if there's a file in the input
  const hasFile = input.backgroundImage instanceof File;

  if (hasFile) {
    // Extract the file and remove it from input
    const backgroundImageFile = input.backgroundImage;
    const inputWithoutFile = { ...input };
    delete inputWithoutFile.backgroundImage;

    // Use the utility function for file upload with separate file parameter
    return await uploadWithFile(mutation, {
      input: inputWithoutFile,
      backgroundImage: backgroundImageFile,
    });
  } else {
    // Regular GraphQL request without files
    const inputWithoutFile = { ...input };
    delete inputWithoutFile.backgroundImage;

    try {
      const response = await axiosWithAuth.post("", {
        query: mutation,
        variables: {
          input: inputWithoutFile,
          backgroundImage: null,
        },
      });
      if (response.data.errors) {
        throw new Error(response.data.errors[0].message);
      }
      return response.data.data.updateOrCreateHeroContent;
    } catch (err) {
      console.error("updateOrCreateHeroContent error:", err);
      throw err;
    }
  }
}

// =============================================================================
// ABOUT US CONTENT FUNCTIONS
// =============================================================================

export async function getAboutUsContents(variables = {}) {
  const query = `
    query GetAboutUsContents($first: Int, $after: String, $where: AboutUsContentFilterInput, $order: [AboutUsContentSortInput!]) {
      aboutUsContents(first: $first, after: $after, where: $where, order: $order) {
        nodes {
          id
          paragraph1
          paragraph2
          vision
          mission
          background
          createdAt
          updatedAt
        }
        pageInfo {
          hasNextPage
          hasPreviousPage
          startCursor
          endCursor
        }
        totalCount
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables,
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.aboutUsContents;
  } catch (err) {
    console.error("getAboutUsContents error:", err);
    throw err;
  }
}

export async function getActiveAboutUsContent() {
  const query = `
    query GetActiveAboutUsContent {
      activeAboutUsContent {
        id
        paragraph1
        paragraph2
        vision
        mission
        background
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables: {},
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.activeAboutUsContent;
  } catch (err) {
    console.error("getActiveAboutUsContent error:", err);
    throw err;
  }
}

export async function updateOrCreateAboutUsContent(input) {
  const mutation = `
    mutation UpdateOrCreateAboutUsContent($input: UpdateAboutUsContentInput!) {
      updateOrCreateAboutUsContent(input: $input) {
        id
        paragraph1
        paragraph2
        vision
        mission
        background
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query: mutation,
      variables: {
        input,
      },
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.updateOrCreateAboutUsContent;
  } catch (err) {
    console.error("updateOrCreateAboutUsContent error:", err);
    throw err;
  }
}

// =============================================================================
// PROMOTION CONTENT FUNCTIONS
// =============================================================================

export async function getPromotionContents(variables = {}) {
  const query = `
    query GetPromotionContents($first: Int, $after: String, $where: PromotionContentFilterInput, $order: [PromotionContentSortInput!]) {
      promotionContents(first: $first, after: $after, where: $where, order: $order) {
        nodes {
          id
          title
          rules
          createdAt
          updatedAt
        }
        pageInfo {
          hasNextPage
          hasPreviousPage
          startCursor
          endCursor
        }
        totalCount
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables,
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.promotionContents;
  } catch (err) {
    console.error("getPromotionContents error:", err);
    throw err;
  }
}

export async function getActivePromotionContent() {
  const query = `
    query GetActivePromotionContent {
      activePromotionContent {
        id
        title
        rules
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables: {},
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.activePromotionContent;
  } catch (err) {
    console.error("getActivePromotionContent error:", err);
    throw err;
  }
}

export async function updateOrCreatePromotionContent(input) {
  const mutation = `
    mutation UpdateOrCreatePromotionContent($input: UpdatePromotionContentInput!) {
      updateOrCreatePromotionContent(input: $input) {
        id
        title
        rules
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query: mutation,
      variables: {
        input,
      },
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.updateOrCreatePromotionContent;
  } catch (err) {
    console.error("updateOrCreatePromotionContent error:", err);
    throw err;
  }
}

// =============================================================================
// CAROUSEL CONTENT FUNCTIONS
// =============================================================================

export async function getCarouselContents(variables = {}) {
  const query = `
    query GetCarouselContents($first: Int, $after: String, $where: CarouselContentFilterInput, $order: [CarouselContentSortInput!]) {
      carouselContents(first: $first, after: $after, where: $where, order: $order) {
        nodes {
          id
          altText
          imageUrl
          createdAt
          updatedAt
        }
        pageInfo {
          hasNextPage
          hasPreviousPage
          startCursor
          endCursor
        }
        totalCount
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables,
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.carouselContents;
  } catch (err) {
    console.error("getCarouselContents error:", err);
    throw err;
  }
}

export async function getAllCarouselContents() {
  const query = `
    query GetAllCarouselContents {
      allCarouselContents {
        id
        altText
        imageUrl
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables: {},
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.allCarouselContents;
  } catch (err) {
    console.error("getAllCarouselContents error:", err);
    throw err;
  }
}

export async function addCarouselContent(altText, image) {
  const mutation = `
    mutation AddCarouselContent($input: CarouselContentInput!) {
      addCarouselContent(input: $input) {
        id
        altText
        imageUrl
        createdAt
        updatedAt
      }
    }
  `;

  const input = {
    altText,
    image,
  };

  // Check if there's a file in the input
  const hasFile = image instanceof File;

  if (hasFile) {
    // Use the utility function for file upload
    return await uploadWithFile(mutation, { input });
  } else {
    // Regular GraphQL request without files
    try {
      const response = await axiosWithAuth.post("", {
        query: mutation,
        variables: { input },
      });
      if (response.data.errors) {
        throw new Error(response.data.errors[0].message);
      }
      return response.data.data.addCarouselContent;
    } catch (err) {
      console.error("addCarouselContent error:", err);
      throw err;
    }
  }
}

export async function deleteCarouselContent(id) {
  const mutation = `
    mutation DeleteCarouselContent($id: String!) {
      deleteCarouselContent(id: $id)
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query: mutation,
      variables: { id },
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.deleteCarouselContent;
  } catch (err) {
    console.error("deleteCarouselContent error:", err);
    throw err;
  }
}

// =============================================================================
// CONTACT CONTENT FUNCTIONS
// =============================================================================

export async function getContactContents(variables = {}) {
  const query = `
    query GetContactContents($first: Int, $after: String, $where: ContactContentFilterInput, $order: [ContactContentSortInput!]) {
      contactContents(first: $first, after: $after, where: $where, order: $order) {
        nodes {
          id
          operationalHours
          address
          whatsapp
          instagram
          googleMaps
          createdAt
          updatedAt
        }
        pageInfo {
          hasNextPage
          hasPreviousPage
          startCursor
          endCursor
        }
        totalCount
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables,
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.contactContents;
  } catch (err) {
    console.error("getContactContents error:", err);
    throw err;
  }
}

export async function getActiveContactContent() {
  const query = `
    query GetActiveContactContent {
      activeContactContent {
        id
        operationalHours
        address
        whatsapp
        instagram
        googleMaps
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query,
      variables: {},
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.activeContactContent;
  } catch (err) {
    console.error("getActiveContactContent error:", err);
    throw err;
  }
}

export async function updateOrCreateContactContent(input) {
  const mutation = `
    mutation UpdateOrCreateContactContent($input: UpdateContactContentInput!) {
      updateOrCreateContactContent(input: $input) {
        id
        operationalHours
        address
        whatsapp
        instagram
        googleMaps
        createdAt
        updatedAt
      }
    }
  `;
  try {
    const response = await axiosWithAuth.post("", {
      query: mutation,
      variables: {
        input,
      },
    });
    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }
    return response.data.data.updateOrCreateContactContent;
  } catch (err) {
    console.error("updateOrCreateContactContent error:", err);
    throw err;
  }
}

// =============================================================================
// UTILITY FUNCTIONS FOR FILE UPLOADS
// =============================================================================

/**
 * Helper function to upload files with GraphQL mutations
 * @param {string} mutation - The GraphQL mutation string
 * @param {Object} variables - GraphQL variables including file objects
 * @returns {Promise} - Promise resolving to mutation result
 */
export async function uploadWithFile(mutation, variables) {
  try {
    const formData = createFormDataForFileUpload(mutation, variables);

    const response = await axiosWithAuth.post("", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    if (response.data.errors) {
      throw new Error(response.data.errors[0].message);
    }

    // Return the first mutation result (assuming single mutation)
    const mutationName = Object.keys(response.data.data)[0];
    return response.data.data[mutationName];
  } catch (err) {
    console.error("uploadWithFile error:", err);
    throw err;
  }
}

/**
 * Helper function to create a File object for GraphQL file uploads
 * @param {File} file - The file object from an HTML input
 * @returns {Object} - Formatted file object for GraphQL
 */
export function formatFileForUpload(file) {
  return {
    name: file.name,
    type: file.type,
    size: file.size,
    // The actual file data will be handled by the multipart form data
  };
}

/**
 * Helper function to handle multipart form data for file uploads
 * @param {string} query - GraphQL query/mutation string
 * @param {Object} variables - GraphQL variables including file
 * @returns {FormData} - Formatted FormData for axios request
 */
export function createFormDataForFileUpload(query, variables) {
  const formData = new FormData();

  // Extract files from variables
  const files = {};
  let fileIndex = 0;

  const processedVariables = JSON.parse(
    JSON.stringify(variables, (key, value) => {
      if (value instanceof File) {
        const fileKey = fileIndex.toString();
        files[fileKey] = {
          file: value,
          path: `variables.${key}`,
        };
        fileIndex++;
        return null; // Replace file with null in variables
      }
      return value;
    })
  );

  // Add the operations (query + variables)
  const operations = {
    query,
    variables: processedVariables,
  };
  formData.append("operations", JSON.stringify(operations));

  // Add the map
  const map = {};
  Object.keys(files).forEach((fileKey) => {
    map[fileKey] = [files[fileKey].path];
  });
  formData.append("map", JSON.stringify(map));

  // Add the files
  Object.keys(files).forEach((fileKey) => {
    formData.append(fileKey, files[fileKey].file);
  });

  return formData;
}

// =============================================================================
// EXAMPLE USAGE PATTERNS
// =============================================================================

/*
// Hero Content Examples:
const heroContents = await getHeroContents({ first: 10 });
const activeHero = await getActiveHeroContent();

// With file upload:
const updatedHeroWithImage = await updateOrCreateHeroContent({ 
  title: "Welcome to KopiAku!", 
  description: "Best coffee in town",
  backgroundImage: fileObject  // File object
});

// Without file upload (update text only):
const updatedHeroTextOnly = await updateOrCreateHeroContent({ 
  title: "Updated Welcome Message!", 
  description: "Even better coffee experience"
  // no backgroundImage = keeps existing image
});

// About Us Content Examples:
const aboutUsData = await getActiveAboutUsContent();
const updatedAboutUs = await updateOrCreateAboutUsContent({
  paragraph1: "We are passionate about coffee...",
  paragraph2: "Founded in 2020...",
  vision: "To serve the best coffee experience",
  mission: ["Quality first", "Customer satisfaction", "Community focus"],
  background: "linear-gradient(135deg, #8B4513 0%, #A0522D 100%)"
});

// Promotion Content Examples:
const promotions = await getPromotionContents();
const updatedPromo = await updateOrCreatePromotionContent({
  title: "New Year Special 2026",
  rules: [
    "Valid until January 31st, 2026",
    "Cannot be combined with other offers"
  ]
});

// Carousel Content Examples:
const carouselItems = await getAllCarouselContents();
const newCarouselItem = await addCarouselContent("Coffee promotion", fileObject);
const deletedCarousel = await deleteCarouselContent("carousel_id");

// Contact Content Examples:
const contactContents = await getContactContents();
const activeContact = await getActiveContactContent();
const updatedContact = await updateOrCreateContactContent({
  operationalHours: "Mon-Fri: 9AM-9PM, Sat-Sun: 10AM-8PM",
  address: "123 Coffee Street, Jakarta, Indonesia",
  whatsapp: "+62812-3456-7890",
  instagram: "https://instagram.com/kopiaku",
  googleMaps: "https://maps.google.com/maps?q=kopiaku+jakarta"
});
*/
